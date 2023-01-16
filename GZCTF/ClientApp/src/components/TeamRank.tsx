import { FC, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import {
  Avatar,
  Group,
  Card,
  Stack,
  Title,
  Text,
  PaperProps,
  createStyles,
  Progress,
  Skeleton,
  PasswordInput,
} from '@mantine/core'
import { useClipboard } from '@mantine/hooks'
import { showNotification } from '@mantine/notifications'
import { mdiCheck, mdiExclamationThick, mdiKey } from '@mdi/js'
import { Icon } from '@mdi/react'
import api from '@Api'

const useStyle = createStyles((theme) => ({
  number: {
    fontFamily: theme.fontFamilyMonospace,
    fontWeight: 700,
  },
}))

const TeamRank: FC<PaperProps> = (props) => {
  const { id } = useParams()
  const numId = parseInt(id ?? '-1')
  const navigate = useNavigate()
  const { data, error } = api.game.useGameChallengesWithTeamInfo(numId)

  const { classes, theme } = useStyle()

  const clipboard = useClipboard()

  const solved = (data?.rank?.solvedCount ?? 0) / (data?.rank?.challenges?.length ?? 1)

  useEffect(() => {
    if (error?.title?.includes('ended')) {
      navigate(`/games/${numId}`)
      showNotification({
        color: 'yellow',
        message: 'The game has ended',
        icon: <Icon path={mdiExclamationThick} size={1} />,
        disallowClose: true,
      })
    }
  }, [error])

  return (
    <Card {...props} shadow="sm" p="md">
      <Stack spacing={8}>
        <Group spacing="sm" noWrap>
          <Avatar color="cyan" size={50} radius="md" src={data?.rank?.avatar}>
            {data?.rank?.name?.slice(0, 1) ?? 'T'}
          </Avatar>
          <Skeleton visible={!data}>
            <Stack spacing={0}>
              <Title order={4} lineClamp={1}>
                {data?.rank?.name ?? 'Team'}
              </Title>
              {data?.rank?.organization && (
                <Text size="sm" lineClamp={1}>
                  {data.rank.organization}
                </Text>
              )}
            </Stack>
          </Skeleton>
        </Group>
        <Group
          grow
          style={{
            textAlign: 'center',
          }}
        >
          <Stack spacing={2}>
            <Skeleton visible={!data}>
              <Text className={classes.number}>{data?.rank?.rank ?? '0'}</Text>
            </Skeleton>
            <Text size="xs">Total Rank</Text>
          </Stack>
          {data?.rank?.organization && (
            <Stack spacing={2}>
              <Skeleton visible={!data}>
                <Text className={classes.number}>{data?.rank?.organizationRank ?? '0'}</Text>
              </Skeleton>
              <Text size="xs">Rank</Text>
            </Stack>
          )}
          <Stack spacing={2}>
            <Skeleton visible={!data}>
              <Text className={classes.number}>{data?.rank?.score ?? '0'}</Text>
            </Skeleton>
            <Text size="xs">Score</Text>
          </Stack>
          <Stack spacing={2}>
            <Skeleton visible={!data}>
              <Text className={classes.number}>{data?.rank?.solvedCount ?? '0'}</Text>
            </Skeleton>
            <Text size="xs">Solved</Text>
          </Stack>
        </Group>
        <Progress value={solved * 100} />
        <PasswordInput
          value={data?.teamToken}
          readOnly
          icon={<Icon path={mdiKey} size={1} />}
          variant="unstyled"
          onClick={() => {
            clipboard.copy(data?.teamToken)
            showNotification({
              color: 'teal',
               message: 'Team token copied to clipboard',
              icon: <Icon path={mdiCheck} size={1} />,
              disallowClose: true,
            })
          }}
          styles={{
            innerInput: {
              cursor: 'copy',
              fontFamily: theme.fontFamilyMonospace,
            },
          }}
        />
      </Stack>
    </Card>
  )
}

export default TeamRank
