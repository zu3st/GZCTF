import dayjs from 'dayjs'
import { FC, useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import {
  Button,
  Container,
  Group,
  Stack,
  Text,
  Title,
  Center,
  Alert,
  Badge,
  BackgroundImage,
  Anchor,
} from '@mantine/core'
import { useScrollIntoView } from '@mantine/hooks'
import { useModals } from '@mantine/modals'
import { showNotification } from '@mantine/notifications'
import { mdiAlertCircle, mdiCheck, mdiFlagOutline, mdiTimerSand } from '@mdi/js'
import { Icon } from '@mdi/react'
import CustomProgress from '@Components/CustomProgress'
import GameJoinModal from '@Components/GameJoinModal'
import MarkdownRender from '@Components/MarkdownRender'
import WithNavBar from '@Components/WithNavbar'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import { useBannerStyles, useIsMobile } from '@Utils/ThemeOverride'
import { usePageTitle } from '@Utils/usePageTitle'
import { useTeams, useUser } from '@Utils/useUser'
import api, { GameJoinModel, ParticipationStatus } from '@Api'

const GameAlertMap = new Map([
  [
    ParticipationStatus.Pending,
    {
      color: 'yellow',
      icon: mdiTimerSand,
      label: 'You have successfully registered as a member of team {TEAM}',
      content: 'Please wait for staff to review your application',
    },
  ],
  [ParticipationStatus.Accepted, null],
  [
    ParticipationStatus.Denied,
    {
      color: 'red',
      icon: mdiAlertCircle,
      label: 'Your application for team {TEAM} has been denied',
      content: 'Please ensure that you meet the requirements and reapply',
    },
  ],
  [
    ParticipationStatus.Forfeited,
    {
      color: 'red',
      icon: mdiAlertCircle,
      label: 'Your team {TEAM} has been disqualified',
      content: 'Please contact staff for more information',
    },
  ],
  [ParticipationStatus.Unsubmitted, null],
])

const GameActionMap = new Map([
  [ParticipationStatus.Pending, 'Pending'],
  [ParticipationStatus.Accepted, 'Accepted'],
  [ParticipationStatus.Denied, 'Reapply'],
  [ParticipationStatus.Forfeited, 'Forfeited'],
  [ParticipationStatus.Unsubmitted, 'Join Game'],
])

const GetAlert = (status: ParticipationStatus, team: string) => {
  const data = GameAlertMap.get(status)
  if (data) {
    return (
      <Alert
        color={data.color}
        icon={<Icon path={data.icon} />}
        title={data.label.replace('{TEAM}', team)}
      >
        {data.content}
      </Alert>
    )
  }
  return null
}

const GameDetail: FC = () => {
  const { id } = useParams()
  const numId = parseInt(id ?? '-1')
  const navigate = useNavigate()

  const {
    data: game,
    error,
    mutate,
  } = api.game.useGameGames(numId, {
    refreshInterval: 0,
    revalidateOnFocus: false,
  })

  const { classes, theme } = useBannerStyles()

  const startTime = dayjs(game?.start) ?? dayjs()
  const endTime = dayjs(game?.end) ?? dayjs()

  const duriation = endTime.diff(startTime, 'minute')
  const current = dayjs().diff(startTime, 'minute')

  const finished = current > duriation
  const started = current > 0
  const progress = started ? (finished ? 100 : current / duriation) : 0

  const { user } = useUser()
  const { teams } = useTeams()

  usePageTitle(game?.title)

  useEffect(() => {
    if (error) {
      showErrorNotification(error)
      navigate('/games')
    }
  }, [error])

  const { scrollIntoView, targetRef } = useScrollIntoView<HTMLDivElement>()

  useEffect(() => scrollIntoView({ alignment: 'center' }), [])

  const status = game?.status ?? ParticipationStatus.Unsubmitted
  const modals = useModals()
  const { isMobile } = useIsMobile()

  const [joinModalOpen, setJoinModalOpen] = useState(false)

  const onSubmitJoin = async (info: GameJoinModel) => {
    try {
      if (!numId) return

      await api.game.gameJoinGame(numId, info)
      showNotification({
        color: 'teal',
        message: 'Join Success',
        icon: <Icon path={mdiCheck} size={1} />,
        disallowClose: true,
      })
      mutate()
    } catch (err) {
      return showErrorNotification(err)
    }
  }

  const onSubmitLeave = async () => {
    try {
      if (!numId) return
      await api.game.gameLeaveGame(numId)

      showNotification({
        color: 'teal',
        message: 'Leave Success',
        icon: <Icon path={mdiCheck} size={1} />,
        disallowClose: true,
      })
      mutate()
    } catch (err) {
      return showErrorNotification(err)
    }
  }

  const canSubmit =
    (status === ParticipationStatus.Unsubmitted || status === ParticipationStatus.Denied) &&
    !finished &&
    user &&
    teams &&
    teams.length > 0

  const teamRequire =
    user && status === ParticipationStatus.Unsubmitted && !finished && teams && teams.length === 0

  const onJoin = () =>
    modals.openConfirmModal({
      title: 'Confirm Join',
      children: (
        <Stack spacing="xs">
          <Text size="sm">Are you sure you want to join this game?</Text>
          <Text size="sm">
            Once joined, no further changes can be made to the team.
            <Text span weight={700}>
            That is, invite or kick out members.
            </Text>
            The team will be unlocked after the game ends or the request is rejected.
          </Text>
          <Text size="sm">
            Team size limit refers to the number of players who can join the game within a team,
            <Text span weight={700}>
              not the number of players in that team.
            </Text>
          </Text>
        </Stack>
      ),
      onConfirm: () => setJoinModalOpen(true),
      centered: true,
      labels: { confirm: 'Confirm', cancel: 'Cancel' },
      confirmProps: { color: 'brand' },
    })

  const onLeave = () =>
    modals.openConfirmModal({
      title: 'Confirm Leave',
      children: (
        <Stack spacing="xs">
          <Text size="sm">Are you sure you want to withdraw from this competition?</Text>
          <Text size="sm">If all members of your team withdraw, the team participation will be canceled.</Text>
        </Stack>
      ),
      onConfirm: onSubmitLeave,
      centered: true,
      labels: { confirm: 'Confirm', cancel: 'Cancel' },
      confirmProps: { color: 'brand' },
    })

  const ControlButtons = (
    <>
      <Button disabled={!canSubmit} onClick={onJoin}>
        {finished ? 'Game Finished' : !user ? 'Please Login' : GameActionMap.get(status)}
      </Button>
      {started && !isMobile && (
        <Button onClick={() => navigate(`/games/${numId}/scoreboard`)}>View Scoreboard</Button>
      )}
      {(status === ParticipationStatus.Pending || status === ParticipationStatus.Denied) && (
        <Button color="red" variant="outline" onClick={onLeave}>
          Withdraw from game
        </Button>
      )}
      {status === ParticipationStatus.Accepted &&
        started &&
        !isMobile &&
        (!finished || game?.practiceMode) && (
          <Button onClick={() => navigate(`/games/${numId}/challenges`)}>Join Game</Button>
        )}
    </>
  )

  return (
    <WithNavBar width="100%" padding={0} isLoading={!game} minWidth={0}>
      <div ref={targetRef} className={classes.root}>
        <Group
          noWrap
          position="apart"
          style={{ width: '100%', padding: `0 ${theme.spacing.md}px` }}
          className={classes.container}
        >
          <Stack spacing={6} className={classes.flexGrowAtSm}>
            <Group>
              <Badge variant="outline">
                {game?.limit === 0 ? 'Team' : game?.limit === 1 ? 'Solo' : game?.limit} Game
              </Badge>
              {game?.hidden && <Badge variant="outline">Hidden</Badge>}
            </Group>
            <Stack spacing={2}>
              <Title className={classes.title}>{game?.title}</Title>
              <Text size="sm" color="dimmed">
                <Text span weight={700}>
                  {`${game?.teamCount ?? 0} `}
                </Text>
                Teams have joined
              </Text>
            </Stack>
            <Group position="apart">
              <Stack spacing={0}>
                <Text size="sm" className={classes.date}>
                  Start time
                </Text>
                <Text size="sm" weight={700} className={classes.date}>
                  {startTime.format('HH:mm:ss, MMMM DD, YYYY')}
                </Text>
              </Stack>
              <Stack spacing={0}>
                <Text size="sm" className={classes.date}>
                  End time
                </Text>
                <Text size="sm" weight={700} className={classes.date}>
                  {endTime.format('HH:mm:ss, MMMM DD, YYYY')}
                </Text>
              </Stack>
            </Group>
            <CustomProgress percentage={progress * 100} />
            <Group>{ControlButtons}</Group>
          </Stack>
          <BackgroundImage className={classes.banner} src={game?.poster ?? ''} radius="sm">
            <Center style={{ height: '100%' }}>
              {!game?.poster && (
                <Icon path={mdiFlagOutline} size={4} color={theme.colors.gray[5]} />
              )}
            </Center>
          </BackgroundImage>
        </Group>
      </div>
      <Container className={classes.content}>
        <Stack spacing="xs">
          {GetAlert(status, game?.teamName ?? '')}
          {teamRequire && (
            <Alert color="yellow" icon={<Icon path={mdiAlertCircle} />} title="Unable to join the game">
              You are not in a team. Please&nbsp;
              <Anchor component={Link} to="/teams">
                join or create a team
              </Anchor>
              &nbsp;first.
            </Alert>
          )}
          {status === ParticipationStatus.Accepted && !started && (
            <Alert color="teal" icon={<Icon path={mdiCheck} />} title="Game has not started yet">
              You have joined the game for team "{game?.teamName}", please wait for the game to start.
              {isMobile && 'Please use a computer to participate in the game and view the game details.'}
            </Alert>
          )}
          <MarkdownRender source={game?.content ?? ''} style={{ marginBottom: 100 }} />
        </Stack>
        <GameJoinModal
          title="Join Game"
          opened={joinModalOpen}
          centered
          withCloseButton={false}
          onClose={() => setJoinModalOpen(false)}
          onSubmitJoin={onSubmitJoin}
          currentOrganization={game?.organization}
        />
      </Container>
    </WithNavBar>
  )
}

export default GameDetail
