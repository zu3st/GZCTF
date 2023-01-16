import { FC, useEffect, useState } from 'react'
import {
  Avatar,
  Box,
  Button,
  Center,
  Grid,
  Group,
  Image,
  Modal,
  ModalProps,
  Stack,
  Text,
  Textarea,
  TextInput,
  useMantineTheme,
  PasswordInput,
  ActionIcon,
  ScrollArea,
  Tooltip,
} from '@mantine/core'
import { Dropzone } from '@mantine/dropzone'
import { useClipboard } from '@mantine/hooks'
import { useModals } from '@mantine/modals'
import { showNotification } from '@mantine/notifications'
import { mdiCheck, mdiClose, mdiRefresh, mdiStar } from '@mdi/js'
import { Icon } from '@mdi/react'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import { ACCEPT_IMAGE_MIME_TYPE } from '@Utils/ThemeOverride'
import api, { TeamInfoModel, TeamUserInfoModel } from '@Api'

interface TeamEditModalProps extends ModalProps {
  team: TeamInfoModel | null
  isCaptain: boolean
}

interface TeamMemberInfoProps {
  user: TeamUserInfoModel
  isCaptain: boolean
  onTransferCaptain: (user: TeamUserInfoModel) => void
  onKick: (user: TeamUserInfoModel) => void
}

const TeamMemberInfo: FC<TeamMemberInfoProps> = (props) => {
  const { user, isCaptain, onKick, onTransferCaptain } = props
  const theme = useMantineTheme()
  const [showBtns, setShowBtns] = useState(false)

  return (
    <Group
      position="apart"
      onMouseEnter={() => setShowBtns(true)}
      onMouseLeave={() => setShowBtns(false)}
    >
      <Group position="left">
        <Avatar src={user.avatar} radius="xl" />
        <Text weight={500}>{user.userName}</Text>
      </Group>
      {isCaptain && showBtns && (
        <Group spacing="xs" position="right">
          <Tooltip label="Transfer Ownership">
            <ActionIcon variant="transparent" onClick={() => onTransferCaptain(user)}>
              <Icon path={mdiStar} size={1} color={theme.colors.yellow[4]} />
            </ActionIcon>
          </Tooltip>
          <Tooltip label="Kick">
            <ActionIcon variant="transparent" onClick={() => onKick(user)}>
              <Icon path={mdiClose} size={1} color={theme.colors.alert[4]} />
            </ActionIcon>
          </Tooltip>
        </Group>
      )}
    </Group>
  )
}

const TeamEditModal: FC<TeamEditModalProps> = (props) => {
  const { team, isCaptain, ...modalProps } = props

  const teamId = team?.id

  const [teamInfo, setTeamInfo] = useState<TeamInfoModel | null>(team)
  const [dropzoneOpened, setDropzoneOpened] = useState(false)
  const [avatarFile, setAvatarFile] = useState<File | null>(null)
  const [inviteCode, setInviteCode] = useState('')

  const theme = useMantineTheme()
  const clipboard = useClipboard()
  const captain = teamInfo?.members?.filter((x) => x.captain)[0]
  const crew = teamInfo?.members?.filter((x) => !x.captain)

  const modals = useModals()

  useEffect(() => {
    setTeamInfo(team)
  }, [team])

  useEffect(() => {
    if (isCaptain && !inviteCode && teamId) {
      api.team.teamInviteCode(teamId).then((code) => {
        setInviteCode(code.data)
      })
    }
  }, [inviteCode, isCaptain, teamId])

  const onConfirmLeaveTeam = () => {
    if (teamInfo && !isCaptain) {
      api.team
        .teamLeave(teamInfo.id!)
        .then(() => {
          showNotification({
            color: 'teal',
            title: 'You have left the team',
            message: 'Team has been updated',
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })
          api.team.mutateTeamGetTeamsInfo()
          props.onClose()
        })
        .catch(showErrorNotification)
    }
  }

  const onConfirmDisbandTeam = () => {
    if (teamInfo && isCaptain) {
      api.team
        .teamDeleteTeam(teamInfo.id!)
        .then(() => {
          showNotification({
            color: 'teal',
            title: 'Team Disbanded',
            message: 'Team has been updated',
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })
          setInviteCode('')
          setTeamInfo(null)
          api.team.mutateTeamGetTeamsInfo()
          props.onClose()
        })
        .catch(showErrorNotification)
    }
  }

  const onTransferCaptain = (userId: string) => {
    if (teamInfo && isCaptain) {
      api.team
        .teamTransfer(teamInfo.id!, {
          newCaptainId: userId,
        })
        .then((team) => {
          showNotification({
            color: 'teal',
            title: 'Team transfered',
            message: 'Team has been updated',
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })
          api.team.mutateTeamGetTeamsInfo()
          setTeamInfo(team.data)
        })
        .catch(showErrorNotification)
    }
  }

  const onConfirmKickUser = (userId: string) => {
    api.team
      .teamKickUser(teamInfo?.id!, userId)
      .then((data) => {
        showNotification({
          color: 'teal',
          title: 'User kicked',
          message: 'Team has been updated',
          icon: <Icon path={mdiCheck} size={1} />,
          disallowClose: true,
        })
        api.team.mutateTeamGetTeamsInfo()
        setTeamInfo(data.data)
      })
      .catch(showErrorNotification)
  }

  const onRefreshInviteCode = () => {
    if (inviteCode) {
      api.team
        .teamUpdateInviteToken(team?.id!)
        .then((data) => {
          setInviteCode(data.data)
          showNotification({
            color: 'teal',
            message: 'Invite code has been updated',
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })
        })
        .catch(showErrorNotification)
    }
  }

  const onChangeAvatar = () => {
    if (avatarFile && teamInfo?.id) {
      api.team
        .teamAvatar(teamInfo?.id, {
          file: avatarFile,
        })
        .then((data) => {
          showNotification({
            color: 'teal',
            message: 'Team avatar has been updated',
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })
          setTeamInfo({ ...teamInfo, avatar: data.data })
          api.team.mutateTeamGetTeamsInfo()
          setAvatarFile(null)
          setDropzoneOpened(false)
        })
        .catch((err) => {
          showErrorNotification(err)
          setDropzoneOpened(false)
        })
    }
  }

  const onSaveChange = () => {
    if (teamInfo && teamInfo?.id) {
      api.team
        .teamUpdateTeam(teamInfo.id, teamInfo)
        .then(() => {
          // Updated TeamInfoModel
          showNotification({
            color: 'teal',
            message: 'Team has been updated',
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })
          api.team.mutateTeamGetTeamsInfo()
        })
        .catch(showErrorNotification)
    }
  }

  return (
    <Modal {...modalProps}>
      <Stack spacing="lg">
        {/* Team Info */}
        <Grid grow>
          <Grid.Col span={8}>
            <TextInput
              label="Team Name"
              type="text"
              placeholder={team?.name ?? 'ctfteam'}
              style={{ width: '100%' }}
              value={teamInfo?.name ?? 'team'}
              disabled={!isCaptain}
              onChange={(event) => setTeamInfo({ ...teamInfo, name: event.target.value })}
            />
          </Grid.Col>
          <Grid.Col span={4}>
            <Center>
              <Avatar
                radius="xl"
                size={70}
                src={teamInfo?.avatar}
                onClick={() => isCaptain && setDropzoneOpened(true)}
              />
            </Center>
          </Grid.Col>
        </Grid>
        {isCaptain && (
          <PasswordInput
            label={
              <Group spacing="xs">
                <Text size="sm">Invite Code</Text>
                <ActionIcon
                  size="sm"
                  onClick={onRefreshInviteCode}
                  sx={(theme) => ({
                    margin: '0 0 -0.1rem -0.5rem',
                    '&:hover': {
                      color:
                        theme.colorScheme === 'dark'
                          ? theme.colors[theme.primaryColor][2]
                          : theme.colors[theme.primaryColor][7],
                      backgroundColor:
                        theme.colorScheme === 'dark' ? theme.colors.dark[7] : theme.white,
                    },
                  })}
                >
                  <Icon path={mdiRefresh} size={1} />
                </ActionIcon>
              </Group>
            }
            value={inviteCode}
            placeholder="loading..."
            onClick={() => {
              clipboard.copy(inviteCode)
              showNotification({
                color: 'teal',
                message: 'Invite code has been copied',
                icon: <Icon path={mdiCheck} size={1} />,
                disallowClose: true,
              })
            }}
            readOnly
          />
        )}

        <Textarea
          label="Team Bio"
          placeholder={teamInfo?.bio ?? 'Apparently, this team prefers to keep an air of mystery about them.'}
          value={teamInfo?.bio ?? 'Apparently, this team prefers to keep an air of mystery about them.'}
          style={{ width: '100%' }}
          disabled={!isCaptain}
          autosize
          minRows={2}
          maxRows={4}
          onChange={(event) => setTeamInfo({ ...teamInfo, bio: event.target.value })}
        />

        <Text size="sm">Member List</Text>
        <ScrollArea style={{ height: 140 }} offsetScrollbars>
          <Stack spacing="xs">
            {captain && (
              <Group position="apart">
                <Group position="left">
                  <Avatar src={captain.avatar} radius="xl" />
                  <Text weight={500}>{captain.userName}</Text>
                </Group>
                <Icon path={mdiStar} size={1} color={theme.colors.yellow[4]} />
              </Group>
            )}
            {crew &&
              crew.map((user) => (
                <TeamMemberInfo
                  key={user.id}
                  isCaptain={isCaptain}
                  user={user}
                  onTransferCaptain={(user: TeamUserInfoModel) => {
                    modals.openConfirmModal({
                      title: 'Confirm Team Transfer',
                      centered: true,
                      children: (
                        <Text size="sm">
                          Are you sure to transfer ownership of team "{teamInfo?.name}" to "{user.userName}"?
                        </Text>
                      ),
                      onConfirm: () => onTransferCaptain(user.id!),
                      labels: { confirm: 'Confirm', cancel: 'Cancel' },
                      confirmProps: { color: 'orange' },
                      zIndex: 10000,
                    })
                  }}
                  onKick={(user: TeamUserInfoModel) => {
                    modals.openConfirmModal({
                      title: 'Confirm Kick',
                      centered: true,
                      children: <Text size="sm">Are you sure to kick out "{user.userName}"?</Text>,
                      onConfirm: () => onConfirmKickUser(user.id!),
                      labels: { confirm: 'Confirm', cancel: 'Cancel' },
                      confirmProps: { color: 'orange' },
                      zIndex: 10000,
                    })
                  }}
                />
              ))}
          </Stack>
        </ScrollArea>

        <Group grow style={{ margin: 'auto', width: '100%' }}>
          <Button
            fullWidth
            color="red"
            variant="outline"
            onClick={() => {
              modals.openConfirmModal({
                title: isCaptain ? 'Confirm Disaband' : 'Confirm Leave',
                centered: true,
                children: isCaptain ? (
                  <Text size="sm">Are you sure to disband the team?</Text>
                ) : (
                  <Text size="sm">Are you sure to leave the team?</Text>
                ),
                onConfirm: isCaptain ? onConfirmDisbandTeam : onConfirmLeaveTeam,
                labels: { confirm: 'Confirm', cancel: 'Cancel' },
                confirmProps: { color: 'red' },
                zIndex: 10000,
              })
            }}
          >
            {isCaptain ? 'Disband Team' : 'Leave Team'}
          </Button>
          <Button fullWidth disabled={!isCaptain} onClick={onSaveChange}>
            Save Change
          </Button>
        </Group>
      </Stack>

      { /* Update avatar modal */ }
      <Modal
        opened={dropzoneOpened}
        onClose={() => setDropzoneOpened(false)}
        centered
        withCloseButton={false}
      >
        <Dropzone
          onDrop={(files) => setAvatarFile(files[0])}
          onReject={() => {
            showNotification({
              color: 'red',
              title: 'Avatar Upload Failed',
              message: 'Please check the file format and size',
              icon: <Icon path={mdiClose} size={1} />,
              disallowClose: true,
            })
          }}
          style={{
            margin: '0 auto 20px auto',
            minWidth: '220px',
            minHeight: '220px',
          }}
          maxSize={3 * 1024 * 1024}
          accept={ACCEPT_IMAGE_MIME_TYPE}
        >
          <Group position="center" spacing="xl" style={{ minHeight: 240, pointerEvents: 'none' }}>
            {avatarFile ? (
              <Image fit="contain" src={URL.createObjectURL(avatarFile)} alt="avatar" />
            ) : (
              <Box>
                <Text size="xl" inline>
                  Drag and drop or click here to select an avatar
                </Text>
                <Text size="sm" color="dimmed" inline mt={7}>
                  Please upload an image file with a maximum size of 3MB
                </Text>
              </Box>
            )}
          </Group>
        </Dropzone>
        <Button fullWidth variant="outline" onClick={onChangeAvatar}>
          Update Avatar
        </Button>
      </Modal>
    </Modal>
  )
}

export default TeamEditModal
