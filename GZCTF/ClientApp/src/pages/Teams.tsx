import { FC, useState } from 'react'
import {
  Stack,
  SimpleGrid,
  Loader,
  Center,
  Group,
  Button,
  Modal,
  TextInput,
  Text,
  useMantineTheme,
  Title,
} from '@mantine/core'
import { showNotification } from '@mantine/notifications'
import { mdiAccountMultiplePlus, mdiCheck, mdiClose, mdiHumanGreetingVariant } from '@mdi/js'
import { Icon } from '@mdi/react'
import LogoHeader from '@Components/LogoHeader'
import TeamCard from '@Components/TeamCard'
import TeamCreateModal from '@Components/TeamCreateModal'
import TeamEditModal from '@Components/TeamEditModal'
import WithNavBar from '@Components/WithNavbar'
import WithRole from '@Components/WithRole'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import { useIsMobile } from '@Utils/ThemeOverride'
import { usePageTitle } from '@Utils/usePageTitle'
import { useTeams, useUser } from '@Utils/useUser'
import api, { TeamInfoModel, Role } from '@Api'

const Teams: FC = () => {
  const { user, error: userError } = useUser()
  const { teams, error: teamsError } = useTeams()

  const theme = useMantineTheme()

  const [joinOpened, setJoinOpened] = useState(false)
  const [joinTeamCode, setJoinTeamCode] = useState('')

  const [createOpened, setCreateOpened] = useState(false)

  const [editOpened, setEditOpened] = useState(false)
  const [editTeam, setEditTeam] = useState<TeamInfoModel | null>(null)

  const ownTeam = teams?.some((t) => t.members?.some((m) => m?.captain && m.id === user?.userId))

  const { isMobile } = useIsMobile()

  const onEditTeam = (team: TeamInfoModel) => {
    setEditTeam(team)
    setEditOpened(true)
  }

  const codePartten = /:\d+:[0-9a-f]{32}$/

  const onJoinTeam = () => {
    if (!codePartten.test(joinTeamCode)) {
      showNotification({
        color: 'red',
        title: 'Encountered a problem',
        message: 'Team invite code format is incorrect',
        icon: <Icon path={mdiClose} size={1} />,
        disallowClose: true,
      })
      return
    }

    api.team
      .teamAccept(joinTeamCode)
      .then(() => {
        showNotification({
          color: 'teal',
          title: 'Joined team successfully',
          message: 'Team info updated',
          icon: <Icon path={mdiCheck} size={1} />,
          disallowClose: true,
        })
        api.team.mutateTeamGetTeamsInfo()
      })
      .catch(showErrorNotification)
      .finally(() => {
        setJoinTeamCode('')
        setJoinOpened(false)
      })
  }

  usePageTitle('Teams')

  const btns = (
    <>
      <Button
        leftIcon={<Icon path={mdiHumanGreetingVariant} size={1} />}
        variant={theme.colorScheme === 'dark' ? 'outline' : 'filled'}
        onClick={() => setJoinOpened(true)}
      >
        Join a team
      </Button>
      <Button
        leftIcon={<Icon path={mdiAccountMultiplePlus} size={1} />}
        variant={theme.colorScheme === 'dark' ? 'outline' : 'filled'}
        onClick={() => setCreateOpened(true)}
      >
        Create a team
      </Button>
    </>
  )

  return (
    <WithNavBar minWidth={0}>
      <WithRole requiredRole={Role.User}>
        <Stack>
          <Group position={isMobile ? 'center' : 'apart'} grow={isMobile}>
            {isMobile ? (
              btns
            ) : (
              <>
                <LogoHeader />
                <Group position="right">{btns}</Group>
              </>
            )}
          </Group>
          {teams && !teamsError && user && !userError ? (
            <>
              <Title
                order={2}
                style={{
                  fontSize: '6rem',
                  fontWeight: 'bold',
                  opacity: 0.15,
                  height: '4.5rem',
                  paddingLeft: '1rem',
                  color: theme.colors.brand[theme.colorScheme === 'dark' ? 2 : 6],
                  userSelect: 'none',
                  marginTop: '-1.5rem',
                }}
              >
                TEAMS
              </Title>
              <SimpleGrid
                cols={3}
                spacing="lg"
                breakpoints={[
                  { maxWidth: 1600, cols: 2, spacing: 'md' },
                  { maxWidth: 800, cols: 1, spacing: 'sm' },
                ]}
              >
                {teams.map((t, i) => (
                  <TeamCard
                    key={i}
                    team={t}
                    isCaptain={t.members?.some((m) => m?.captain && m.id === user?.userId) ?? false}
                    onEdit={() => onEditTeam(t)}
                  />
                ))}
              </SimpleGrid>
            </>
          ) : (
            <Center style={{ width: '100%', height: '80wh' }}>
              <Loader />
            </Center>
          )}
        </Stack>

        <Modal
          opened={joinOpened}
          centered
          title="Join an existing team"
          onClose={() => setJoinOpened(false)}
        >
          <Stack>
            <Text size="sm">Please get the team invite code from the team creator and enter it to join the team.</Text>
            
            <TextInput
              label="Team invite code"
              type="text"
              placeholder="team:0:01234567890123456789012345678901"
              style={{ width: '100%' }}
              value={joinTeamCode}
              onChange={(event) => setJoinTeamCode(event.currentTarget.value)}
            />
            <Button fullWidth variant="outline" onClick={onJoinTeam}>
            Join Team
            </Button>
          </Stack>
        </Modal>

        <TeamCreateModal
          opened={createOpened}
          centered
          title="Create a new team"
          isOwnTeam={ownTeam ?? false}
          onClose={() => setCreateOpened(false)}
        />

        <TeamEditModal
          opened={editOpened}
          centered
          title="Team Details"
          onClose={() => setEditOpened(false)}
          team={editTeam}
          isCaptain={editTeam?.members?.some((m) => m?.captain && m.id === user?.userId) ?? false}
        />
      </WithRole>
    </WithNavBar>
  )
}

export default Teams
