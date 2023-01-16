import dayjs from 'dayjs'
import { FC, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import {
  Button,
  Center,
  Grid,
  Group,
  Input,
  NumberInput,
  Stack,
  Textarea,
  TextInput,
  Image,
  Text,
  MultiSelect,
  ActionIcon,
  Switch,
  PasswordInput,
  SimpleGrid,
} from '@mantine/core'
import { DatePicker, TimeInput } from '@mantine/dates'
import { Dropzone } from '@mantine/dropzone'
import { useClipboard, useInputState } from '@mantine/hooks'
import { useModals } from '@mantine/modals'
import { showNotification } from '@mantine/notifications'
import {
  mdiKeyboardBackspace,
  mdiCheck,
  mdiClose,
  mdiContentSaveOutline,
  mdiRefresh,
  mdiDeleteOutline,
} from '@mdi/js'
import { Icon } from '@mdi/react'
import { SwitchLabel } from '@Components/admin/SwitchLabel'
import WithGameEditTab from '@Components/admin/WithGameEditTab'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import { ACCEPT_IMAGE_MIME_TYPE } from '@Utils/ThemeOverride'
import api, { GameInfoModel } from '@Api'

const GenerateRandomCode = () => {
  const chars = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'
  let code = ''
  for (let i = 0; i < 24; i++) {
    code += chars[Math.floor(Math.random() * chars.length)]
  }
  return code
}

const GameInfoEdit: FC = () => {
  const { id } = useParams()
  const numId = parseInt(id ?? '-1')
  const { data: gameSource, mutate } = api.edit.useEditGetGame(numId, {
    refreshInterval: 0,
    revalidateIfStale: false,
    revalidateOnFocus: false,
  })
  const [game, setGame] = useState<GameInfoModel>()
  const navigate = useNavigate()

  const [disabled, setDisabled] = useState(false)
  const [organizations, setOrganizations] = useState<string[]>([])
  const [start, setStart] = useInputState(dayjs())
  const [end, setEnd] = useInputState(dayjs())
  const [wpddl, setWpddl] = useInputState(3)

  const modals = useModals()
  const clipboard = useClipboard()

  useEffect(() => {
    if (numId < 0) {
      showNotification({
        color: 'red',
        message: `Invalid game id: ${id}`,
        icon: <Icon path={mdiClose} size={1} />,
        disallowClose: true,
      })
      navigate('/admin/games')
      return
    }

    if (gameSource) {
      setGame(gameSource)
      setStart(dayjs(gameSource.start))
      setEnd(dayjs(gameSource.end))
      setWpddl(dayjs(gameSource.wpddl).diff(gameSource.end, 'h'))
      setOrganizations(gameSource.organizations || [])
    }
  }, [id, gameSource])

  const onUpdatePoster = (file: File | undefined) => {
    if (game && file) {
      api.edit
        .editUpdateGamePoster(game.id!, { file })
        .then((res) => {
          showNotification({
            color: 'teal',
            message: 'Successfully updated game poster',
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })
          mutate({ ...game, poster: res.data })
        })
        .catch(showErrorNotification)
    }
  }

  const onUpdateInfo = () => {
    if (game && game.title) {
      setDisabled(true)
      api.edit
        .editUpdateGame(game.id!, {
          ...game,
          inviteCode: game.inviteCode?.length ?? 0 > 6 ? game.inviteCode : null,
          start: start.toJSON(),
          end: end.toJSON(),
          wpddl: end.add(wpddl, 'h').toJSON(),
        })
        .then(() => {
          showNotification({
            color: 'teal',
            message: 'Game updated',
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })
          mutate()
          api.game.mutateGameGamesAll()
        })
        .catch(showErrorNotification)
        .finally(() => {
          setDisabled(false)
        })
    }
  }

  const onConfirmDelete = () => {
    if (game) {
      api.edit
        .editDeleteGame(game.id!)
        .then(() => {
          showNotification({
            color: 'teal',
            message: 'Game deleted',
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })
          navigate('/admin/games')
        })
        .catch(showErrorNotification)
    }
  }

  return (
    <WithGameEditTab
      headProps={{ position: 'apart' }}
      isLoading={!game}
      head={
        <>
          <Button
            leftIcon={<Icon path={mdiKeyboardBackspace} size={1} />}
            onClick={() => navigate('/admin/games')}
          >
            Back
          </Button>
          <Group position="right">
            <Button
              disabled={disabled}
              color="red"
              leftIcon={<Icon path={mdiDeleteOutline} size={1} />}
              variant="outline"
              onClick={() =>
                modals.openConfirmModal({
                  title: 'Delete Game',
                  children: <Text size="sm">Are you sure to delete game "{game?.title}"?</Text>,
                  onConfirm: () => onConfirmDelete(),
                  centered: true,
                  labels: { confirm: 'Confirm', cancel: 'Cancel' },
                  confirmProps: { color: 'red' },
                })
              }
            >
              Delete Game
            </Button>
            <Button
              leftIcon={<Icon path={mdiContentSaveOutline} size={1} />}
              disabled={disabled}
              onClick={onUpdateInfo}
            >
              Save Changes
            </Button>
          </Group>
        </>
      }
    >
      <SimpleGrid cols={4}>
        <TextInput
          label="Game Title"
          description="Game title, will be displayed on the games page"
          disabled={disabled}
          value={game?.title}
          required
          onChange={(e) => game && setGame({ ...game, title: e.target.value })}
        />
        <NumberInput
          label="Team Member Count Limit"
          description="Set to 0 to disable team member limit"
          disabled={disabled}
          min={0}
          required
          value={game?.teamMemberCountLimit}
          onChange={(e) => game && setGame({ ...game, teamMemberCountLimit: e })}
        />
        <NumberInput
          label="Team Container Count Limit"
          description="Limit the number of shared containers per team"
          disabled={disabled}
          min={1}
          required
          value={game?.containerCountLimit}
          onChange={(e) => game && setGame({ ...game, containerCountLimit: e })}
        />
        <PasswordInput
          value={game?.publicKey || ''}
          label="Game Signature Public Key"
          description="Used to verify team token"
          readOnly
          onClick={() => {
            clipboard.copy(game?.publicKey || '')
            showNotification({
              color: 'teal',
              message: 'Public key copied to clipboard',
              icon: <Icon path={mdiCheck} size={1} />,
              disallowClose: true,
            })
          }}
          styles={{
            innerInput: {
              cursor: 'copy',
            },
          }}
        />
        <DatePicker
          label="Start Date"
          placeholder="Start Date"
          value={start.toDate()}
          disabled={disabled}
          clearable={false}
          onChange={(e) => {
            const newDate = dayjs(e)
              .hour(start.hour())
              .minute(start.minute())
              .second(start.second())
            setStart(newDate)
            if (newDate && end < newDate) {
              setEnd(newDate.add(2, 'h'))
            }
          }}
          required
        />
        <TimeInput
          label="Start Time"
          disabled={disabled}
          placeholder="Start Time"
          value={start.toDate()}
          onChange={(e) => {
            const newDate = dayjs(e).date(start.date()).month(start.month()).year(start.year())
            setStart(newDate)
            if (newDate && end < newDate) {
              setEnd(newDate.add(2, 'h'))
            }
          }}
          withSeconds
          required
        />
        <DatePicker
          label="End Date"
          disabled={disabled}
          minDate={start.toDate()}
          placeholder="End Date"
          value={end.toDate()}
          clearable={false}
          onChange={(e) => {
            const newDate = dayjs(e).hour(end.hour()).minute(end.minute()).second(end.second())
            setEnd(newDate)
          }}
          error={end < start}
          required
        />
        <TimeInput
          label="End Time"
          disabled={disabled}
          placeholder="End Time"
          value={end.toDate()}
          onChange={(e) => {
            const newDate = dayjs(e).date(end.date()).month(end.month()).year(end.year())
            setEnd(newDate)
          }}
          error={end < start}
          withSeconds
          required
        />
      </SimpleGrid>
      <Grid>
        <Grid.Col span={6}>
          <Textarea
            label="Game Summary"
            description="Will be displayed on the games page"
            value={game?.summary}
            style={{ width: '100%' }}
            autosize
            disabled={disabled}
            minRows={3}
            maxRows={3}
            onChange={(e) => game && setGame({ ...game, summary: e.target.value })}
          />
        </Grid.Col>
        <Grid.Col span={3}>
          <Stack spacing="xs">
            <TextInput
              label="Invite Code"
              description="Leave blank to disable invite code"
              value={game?.inviteCode || ''}
              disabled={disabled}
              onChange={(e) => game && setGame({ ...game, inviteCode: e.target.value })}
              rightSection={
                <ActionIcon
                  onClick={() => game && setGame({ ...game, inviteCode: GenerateRandomCode() })}
                >
                  <Icon path={mdiRefresh} size={1} />
                </ActionIcon>
              }
            />
            <Switch
              disabled={disabled}
              checked={game?.acceptWithoutReview ?? false}
              label={SwitchLabel('Accept Teams Without Review', 'Teams will be accepted without review')}
              onChange={(e) => game && setGame({ ...game, acceptWithoutReview: e.target.checked })}
            />
          </Stack>
        </Grid.Col>
        <Grid.Col span={3}>
          <Stack spacing="xs">
            <NumberInput
              label="Writeup Submission Deadline"
              description="Hours after game end until writeups are no longer accepted"
              disabled={disabled}
              min={0}
              required
              value={wpddl}
              onChange={setWpddl}
            />
            <Switch
              disabled={disabled}
              checked={game?.practiceMode ?? true}
              label={SwitchLabel('Practice Mode', 'Allow teams to continue playing after the game ends')}
              onChange={(e) => game && setGame({ ...game, practiceMode: e.target.checked })}
            />
          </Stack>
        </Grid.Col>
      </Grid>
      <Group grow position="apart">
        <Textarea
          label={
            <Group spacing="sm">
              <Text size="sm">Writeup Additional Notes</Text>
              <Text size="xs" color="dimmed">
                Supports Markdown syntax
              </Text>
            </Group>
          }
          value={game?.wpNote}
          style={{ width: '100%' }}
          autosize
          disabled={disabled}
          minRows={3}
          maxRows={3}
          onChange={(e) => game && setGame({ ...game, wpNote: e.target.value })}
        />
        <MultiSelect
          label={
            <Group spacing="sm">
              <Text size="sm">Participating Organizations</Text>
              <Text size="xs" color="dimmed">
                Enable group leaderboard by adding participating organizations
              </Text>
            </Group>
          }
          searchable
          creatable
          disabled={disabled}
          placeholder="Leave blank to allow teams without organization"
          maxDropdownHeight={300}
          value={game?.organizations ?? []}
          styles={{
            input: {
              minHeight: 88,
              maxHeight: 88,
            },
          }}
          onChange={(e) => game && setGame({ ...game, organizations: e })}
          data={organizations.map((o) => ({ value: o, label: o })) || []}
          getCreateLabel={(query) => `+ Add Organization "${query}"`}
          onCreate={(query) => {
            const item = { value: query, label: query }
            setOrganizations([...organizations, query])
            return item
          }}
        />
      </Group>
      <Grid grow>
        <Grid.Col span={8}>
          <Textarea
            label={
              <Group spacing="sm">
                <Text size="sm">Game Details</Text>
                <Text size="xs" color="dimmed">
                  Supports Markdown syntax
                </Text>
              </Group>
            }
            value={game?.content}
            style={{ width: '100%' }}
            autosize
            disabled={disabled}
            minRows={8}
            maxRows={8}
            onChange={(e) => game && setGame({ ...game, content: e.target.value })}
          />
        </Grid.Col>
        <Grid.Col span={4}>
          <Input.Wrapper label="Game Poster">
            <Dropzone
              onDrop={(files) => onUpdatePoster(files[0])}
              onReject={() => {
                showNotification({
                  color: 'red',
                  title: 'Poster Upload Failed',
                  message: 'Please check the file format and size',
                  icon: <Icon path={mdiClose} size={1} />,
                  disallowClose: true,
                })
              }}
              maxSize={3 * 1024 * 1024}
              accept={ACCEPT_IMAGE_MIME_TYPE}
              disabled={disabled}
              styles={{
                root: {
                  height: '198px',
                  padding: game?.poster ? '0' : '16px',
                },
              }}
            >
              <Center style={{ pointerEvents: 'none' }}>
                {game?.poster ? (
                  <Image height="195px" fit="contain" src={game.poster} />
                ) : (
                  <Center style={{ height: '160px' }}>
                    <Stack spacing={0}>
                      <Text size="xl" inline>
                      Drag and drop or click here to select a poster
                      </Text>
                      <Text size="sm" color="dimmed" inline mt={7}>
                        Please upload an image file with a maximum size of 3MB
                      </Text>
                    </Stack>
                  </Center>
                )}
              </Center>
            </Dropzone>
          </Input.Wrapper>
        </Grid.Col>
      </Grid>
    </WithGameEditTab>
  )
}

export default GameInfoEdit
