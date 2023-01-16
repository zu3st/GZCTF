import { FC, useState } from 'react'
import {
  Button,
  Modal,
  ModalProps,
  Stack,
  Textarea,
  TextInput,
  Text,
  Title,
  Center,
  useMantineTheme,
} from '@mantine/core'
import { showNotification } from '@mantine/notifications'
import { mdiCloseCircle, mdiCheck } from '@mdi/js'
import { Icon } from '@mdi/react'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import api, { TeamUpdateModel } from '@Api'

interface TeamEditModalProps extends ModalProps {
  isOwnTeam: boolean
}

const TeamCreateModal: FC<TeamEditModalProps> = (props) => {
  const { isOwnTeam, ...modalProps } = props
  const [createTeam, setCreateTeam] = useState<TeamUpdateModel>({ name: '', bio: '' })
  const theme = useMantineTheme()

  const onCreateTeam = () => {
    api.team
      .teamCreateTeam(createTeam)
      .then((res) => {
        showNotification({
          color: 'teal',
          title: 'Team created',
          message: `${res.data.name} created, it's time to invite your friends!`,
          icon: <Icon path={mdiCheck} size={1} />,
          disallowClose: true,
        })
        api.team.mutateTeamGetTeamsInfo()
      })
      .catch(showErrorNotification)
      .finally(() => {
        modalProps.onClose()
      })
  }

  return (
    <Modal {...modalProps}>
      {isOwnTeam ? (
        <Stack spacing="lg" p={40} style={{ textAlign: 'center' }}>
          <Center>
            <Icon color={theme.colors.red[7]} path={mdiCloseCircle} size={4} />
          </Center>
          <Title order={3}>You already own a team</Title>
          <Text>
            Each user can only own one team.
            <br />
            You can delete it and create a new one.
          </Text>
        </Stack>
      ) : (
        <Stack>
          <Text>
            Create a team, you can organize a team and invite others to join. Each user can only own one team.
          </Text>
          <TextInput
            label="Team Name"
            type="text"
            placeholder="team"
            style={{ width: '100%' }}
            value={createTeam?.name ?? ''}
            onChange={(event) => setCreateTeam({ ...createTeam, name: event.currentTarget.value })}
          />
          <Textarea
            label="Team Bio"
            placeholder={createTeam?.bio ?? 'Apparently, this team prefers to keep an air of mystery about them.'}
            value={createTeam?.bio ?? ''}
            style={{ width: '100%' }}
            autosize
            minRows={2}
            maxRows={4}
            onChange={(event) => setCreateTeam({ ...createTeam, bio: event.currentTarget.value })}
          />
          <Button fullWidth variant="outline" onClick={onCreateTeam}>
            Create Team
          </Button>
        </Stack>
      )}
    </Modal>
  )
}

export default TeamCreateModal
