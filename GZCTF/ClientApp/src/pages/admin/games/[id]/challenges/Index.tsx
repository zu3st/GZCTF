import { Dispatch, FC, SetStateAction, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import {
  Button,
  Center,
  Group,
  ScrollArea,
  Select,
  SimpleGrid,
  Stack,
  Text,
  Title,
} from '@mantine/core'
import { useModals } from '@mantine/modals'
import { showNotification } from '@mantine/notifications'
import { mdiKeyboardBackspace, mdiCheck, mdiPlus, mdiHexagonSlice6 } from '@mdi/js'
import { Icon } from '@mdi/react'
import BloodBonusModel from '@Components/admin/BloodBonusModel'
import ChallengeCreateModal from '@Components/admin/ChallengeCreateModal'
import ChallengeEditCard from '@Components/admin/ChallengeEditCard'
import WithGameEditTab from '@Components/admin/WithGameEditTab'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import { ChallengeTagItem, ChallengeTagLabelMap } from '@Utils/ChallengeItem'
import api, { ChallengeInfoModel, ChallengeTag } from '@Api'

const GameChallengeEdit: FC = () => {
  const { id } = useParams()
  const numId = parseInt(id ?? '-1')

  const navigate = useNavigate()
  const [createOpened, setCreateOpened] = useState(false)
  const [bonusOpened, setBonusOpened] = useState(false)
  const [category, setCategory] = useState<ChallengeTag | null>(null)

  const { data: challenges, mutate } = api.edit.useEditGetGameChallenges(numId, {
    refreshInterval: 0,
    revalidateIfStale: false,
    revalidateOnFocus: false,
  })

  const filteredChallenges =
    category && challenges ? challenges?.filter((c) => c.tag === category) : challenges
  filteredChallenges?.sort((a, b) => ((a.tag ?? '') > (b.tag ?? '') ? -1 : 1))

  const modals = useModals()
  const onToggle = (
    challenge: ChallengeInfoModel,
    setDisabled: Dispatch<SetStateAction<boolean>>
  ) => {
    const op = challenge.isEnabled ? 'Disable' : 'Enable'
    modals.openConfirmModal({
      title: `${op} challenge`, 
      children: (
        <Text size="sm">
          Are you sure to {op} challenge "{challenge.title}"?
        </Text>
      ),
      onConfirm: () => onConfirmToggle(challenge, setDisabled),
      centered: true,
      labels: { confirm: 'Confirm', cancel: 'Cancel' },
      confirmProps: { color: 'orange' },
    })
  }

  const onConfirmToggle = (
    challenge: ChallengeInfoModel,
    setDisabled: Dispatch<SetStateAction<boolean>>
  ) => {
    const numId = parseInt(id ?? '-1')
    setDisabled(true)
    api.edit
      .editUpdateGameChallenge(numId, challenge.id!, {
        isEnabled: !challenge.isEnabled,
      })
      .then(() => {
        showNotification({
          color: 'teal',
          message: 'Challenge status updated successfully',
          icon: <Icon path={mdiCheck} size={1} />,
          disallowClose: true,
        })
        mutate(
          challenges?.map((c) =>
            c.id === challenge.id ? { ...c, isEnabled: !challenge.isEnabled } : c
          )
        )
      })
      .catch(showErrorNotification)
      .finally(() => {
        setDisabled(false)
      })
  }

  return (
    <WithGameEditTab
      headProps={{ position: 'apart' }}
      isLoading={!challenges}
      head={
        <>
          <Button
            leftIcon={<Icon path={mdiKeyboardBackspace} size={1} />}
            onClick={() => navigate('/admin/games')}
          >
            Back
          </Button>
          <Group w="calc(100% - 9rem)" position="apart">
            <Select
              placeholder="All challenges"
              clearable
              searchable
              nothingFound="No tags found"
              clearButtonLabel="Show all"
              value={category}
              onChange={(value: ChallengeTag) => setCategory(value)}
              itemComponent={ChallengeTagItem}
              data={Object.entries(ChallengeTag).map((tag) => {
                const data = ChallengeTagLabelMap.get(tag[1])
                return { value: tag[1], ...data }
              })}
            />
            <Group position="right">
              <Button
                leftIcon={<Icon path={mdiHexagonSlice6} size={1} />}
                onClick={() => setBonusOpened(true)}
              >
                First Blood Bonus
              </Button>
              <Button
                style={{ marginRight: '18px' }}
                leftIcon={<Icon path={mdiPlus} size={1} />}
                onClick={() => setCreateOpened(true)}
              >
                Create Challenge
              </Button>
            </Group>
          </Group>
        </>
      }
    >
      <ScrollArea
        style={{ height: 'calc(100vh - 180px)', position: 'relative' }}
        offsetScrollbars
        type="auto"
      >
        {!filteredChallenges || filteredChallenges.length === 0 ? (
          <Center style={{ height: 'calc(100vh - 200px)' }}>
            <Stack spacing={0}>
              <Title order={2}>Ouch! This game has no challenges</Title>
              <Text>Click the button on the top right to create the first challenge</Text>
            </Stack>
          </Center>
        ) : (
          <SimpleGrid
            cols={2}
            pr={6}
            breakpoints={[
              { maxWidth: 3600, cols: 2, spacing: 'sm' },
              { maxWidth: 1800, cols: 1, spacing: 'sm' },
            ]}
          >
            {filteredChallenges &&
              filteredChallenges.map((challenge) => (
                <ChallengeEditCard key={challenge.id} challenge={challenge} onToggle={onToggle} />
              ))}
          </SimpleGrid>
        )}
      </ScrollArea>
      <ChallengeCreateModal
        title="Create Challenge"
        centered
        size="30%"
        opened={createOpened}
        onClose={() => setCreateOpened(false)}
        onAddChallenge={(challenge) => mutate([challenge, ...(challenges ?? [])])}
      />
      <BloodBonusModel
        title="First Blood Bonus"
        centered
        size="30%"
        opened={bonusOpened}
        onClose={() => setBonusOpened(false)}
      />
    </WithGameEditTab>
  )
}

export default GameChallengeEdit
