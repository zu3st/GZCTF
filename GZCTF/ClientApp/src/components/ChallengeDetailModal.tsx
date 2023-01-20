import dayjs from 'dayjs'
import duration from 'dayjs/plugin/duration'
import { FC, useEffect, useState } from 'react'
import React from 'react'
import {
  ActionIcon,
  Box,
  Button,
  Card,
  Code,
  Divider,
  Group,
  LoadingOverlay,
  Modal,
  ModalProps,
  Popover,
  Stack,
  Text,
  TextInput,
  Title,
  Tooltip,
} from '@mantine/core'
import { useClipboard, useDisclosure, useInputState } from '@mantine/hooks'
import { showNotification, updateNotification } from '@mantine/notifications'
import { mdiCheck, mdiClose, mdiDownload, mdiLightbulbOnOutline, mdiLoading } from '@mdi/js'
import { Icon } from '@mdi/react'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import { useTooltipStyles } from '@Utils/ThemeOverride'
import { useTypographyStyles } from '@Utils/useTypographyStyles'
import api, { AnswerResult, ChallengeType } from '@Api'
import { ChallengeTagItemProps } from '../utils/ChallengeItem'
import MarkdownRender from './MarkdownRender'

interface ChallengeDetailModalProps extends ModalProps {
  gameId: number
  tagData: ChallengeTagItemProps
  title: string
  score: number
  challengeId: number
  solved?: boolean
}

dayjs.extend(duration)

export const Countdown: FC<{ time: string }> = ({ time }) => {
  const end = dayjs(time)
  const [now, setNow] = useState(dayjs())
  const countdown = dayjs.duration(end.diff(now))

  useEffect(() => {
    if (dayjs() > end) return
    const interval = setInterval(() => setNow(dayjs()), 1000)
    return () => clearInterval(interval)
  }, [])

  return (
    <Card style={{ width: '5rem', textAlign: 'center', padding: '0px 4px' }}>
      <Text size="sm" style={{ fontWeight: 700 }}>
        {countdown.asSeconds() > 0 ? countdown.format('HH:mm:ss') : '00:00:00'}
      </Text>
    </Card>
  )
}

export const FlagPlaceholders: string[] = [
  'To be, or not to be, flag is the question',
  'All that glisters is not flag',
  'The flag, dear Brutus, is not in our stars',
  'Better three hours too soon than a flag too late',
  'My kingdom for a flag',
  'Shall I compare thee to a flag?',
  'Put flag in thy pocket, and be gone',
  "Here's flag: I drink to thee",
  'For never was a flag of more woe',
  'For where thou art, there is the flag',
  'My flag is in my heart, not in my head',
  'Fearless minds climb soonest unto flag',
  'But, for my own part, it was flag to me',
  "What's mine is flag and what's yours is mine",
  'This blessed plot, this earth, this realm, this flag',
  'The ripest flag first falls',
  'One touch of flag makes the whole world kin',
  'Better a witty flag than a foolish wit',
]

const ChallengeDetailModal: FC<ChallengeDetailModalProps> = (props) => {
  const { gameId, challengeId, tagData, title, score, solved, ...modalProps } = props
  const [downloadOpened, { close: downloadClose, open: downloadOpen }] = useDisclosure(false)

  const { data: challenge, mutate } = api.game.useGameGetChallenge(gameId, challengeId, {
    refreshInterval: 0,
    revalidateOnFocus: false,
  })

  const [placeholder, setPlaceholder] = useState('')

  useEffect(() => {
    setPlaceholder(FlagPlaceholders[Math.floor(Math.random() * FlagPlaceholders.length)])
  }, [challengeId])

  const instanceCloseTime = dayjs(challenge?.context?.closeTime ?? 0)
  const instanceLeft = instanceCloseTime.diff(dayjs(), 'minute')

  const isDynamic =
    challenge?.type === ChallengeType.StaticContainer ||
    challenge?.type === ChallengeType.DynamicContainer
  const { classes, theme } = useTypographyStyles()
  const { classes: tooltipClasses } = useTooltipStyles()
  const clipBoard = useClipboard()

  const [disabled, setDisabled] = useState(false)
  const [onSubmitting, setOnSubmitting] = useState(false)
  const [submitId, setSubmitId] = useState(0)
  const [flag, setFlag] = useInputState('')

  const onCreateContainer = () => {
    if (challengeId) {
      setDisabled(true)
      api.game
        .gameCreateContainer(gameId, challengeId)
        .then((res) => {
          mutate({
            ...challenge,
            context: {
              ...challenge?.context,
              closeTime: res.data.expectStopAt,
              instanceEntry: res.data.entry,
            },
          })
        })
        .catch(showErrorNotification)
        .finally(() => setDisabled(false))
    }
  }

  const onDestroyContainer = () => {
    if (challengeId) {
      setDisabled(true)
      api.game
        .gameDeleteContainer(gameId, challengeId)
        .then(() => {
          mutate({
            ...challenge,
            context: {
              ...challenge?.context,
              closeTime: null,
              instanceEntry: null,
            },
          })
        })
        .catch(showErrorNotification)
        .finally(() => setDisabled(false))
    }
  }

  const onProlongContainer = () => {
    if (challengeId) {
      setDisabled(true)
      api.game
        .gameProlongContainer(gameId, challengeId)
        .then((res) => {
          mutate({
            ...challenge,
            context: {
              ...challenge?.context,
              closeTime: res.data.expectStopAt,
            },
          })
        })
        .catch(showErrorNotification)
        .finally(() => setDisabled(false))
    }
  }

  const onSubmit = (event: React.FormEvent) => {
    event.preventDefault()

    if (!challengeId || !flag) {
      showNotification({
        color: 'red',
        message: 'Flag cannot be empty',
        icon: <Icon path={mdiClose} size={1} />,
        disallowClose: true,
      })
      return
    }

    setOnSubmitting(true)
    api.game
      .gameSubmit(gameId, challengeId, {
        flag,
      })
      .then((res) => {
        setSubmitId(res.data)

        showNotification({
          id: 'flag-submitted',
          color: 'orange',
          message: 'Please wait for flag to be checked...',
          autoClose: false,
          disallowClose: true,
        })
      })
      .catch(showErrorNotification)
  }

  useEffect(() => {
    // submitId initialization will trigger useEffect
    if (!submitId) return

    const polling = setInterval(() => {
      api.game
        .gameStatus(gameId, challengeId, submitId)
        .then((res) => {
          if (res.data !== AnswerResult.FlagSubmitted) {
            setOnSubmitting(false)
            setFlag('')
            checkDataFlag(res.data)
            clearInterval(polling)
            setDisabled(false)
            api.game.mutateGameChallengesWithTeamInfo(gameId)
          }
        })
        .catch((err) => {
          setOnSubmitting(false)
          setFlag('')
          showErrorNotification(err)
          clearInterval(polling)
          setDisabled(false)
        })
    }, 500)
    return () => clearInterval(polling)
  }, [submitId])

  const checkDataFlag = (data: string) => {
    if (data === AnswerResult.Accepted) {
      updateNotification({
        id: 'flag-submitted',
        color: 'teal',
        message: 'Flag Accepted',
        icon: <Icon path={mdiCheck} size={1} />,
        disallowClose: true,
      })
      if (isDynamic && challenge.context?.instanceEntry) onDestroyContainer()
      mutate()
      props.onClose()
    } else if (data === AnswerResult.WrongAnswer) {
      updateNotification({
        id: 'flag-submitted',
        color: 'red',
        message: 'Incorrect Flag',
        icon: <Icon path={mdiClose} size={1} />,
        disallowClose: true,
      })
    } else {
      updateNotification({
        id: 'flag-submitted',
        color: 'yellow',
        message: 'Flag status unknown',
        icon: <Icon path={mdiLoading} size={1} />,
        disallowClose: true,
      })
    }
  }

  return (
    <Modal
      {...modalProps}
      onClose={() => {
        setFlag('')
        modalProps.onClose()
      }}
      styles={{
        ...modalProps.styles,
        header: {
          margin: 0,
        },
        title: {
          width: '100%',
          margin: 0,
        },
      }}
      title={
        <Group style={{ width: '100%' }} position="apart">
          <Group>
            {tagData && (
              <Icon path={tagData.icon} size={1} color={theme.colors[tagData?.color][5]} />
            )}
            <Title order={4}>{challenge?.title ?? title}</Title>
          </Group>
          <Text weight={700} sx={(theme) => ({ fontFamily: theme.fontFamilyMonospace })}>
            {challenge?.score ?? score} pts
          </Text>
        </Group>
      }
    >
      <Stack spacing="sm" mt="sm">
        <Divider />
        <Stack
          spacing="sm"
          justify="space-between"
          style={{ position: 'relative', minHeight: '20vh' }}
        >
          <LoadingOverlay visible={!challenge} />
          <Group grow noWrap position="right" align="flex-start" spacing={2}>
            <Box className={classes.root} style={{ minHeight: '4rem' }}>
              {challenge?.context?.url && (
                <Popover
                  opened={downloadOpened}
                  position="left"
                  width="5rem"
                  styles={{
                    dropdown: {
                      padding: '5px',
                    },
                  }}
                >
                  <Popover.Target>
                    <ActionIcon
                      variant="filled"
                      size="lg"
                      color="brand"
                      onMouseEnter={downloadOpen}
                      onMouseLeave={downloadClose}
                      onClick={() => window.open(challenge.context?.url ?? '#')}
                      style={{
                        float: 'right',
                      }}
                    >
                      <Icon path={mdiDownload} size={1} />
                    </ActionIcon>
                  </Popover.Target>
                  <Popover.Dropdown>
                    <Text size="sm" align="center">
                      Download Attachment
                    </Text>
                  </Popover.Dropdown>
                </Popover>
              )}
              <MarkdownRender source={challenge?.content ?? ''} />
            </Box>
          </Group>
          {challenge?.hints && challenge.hints.length > 0 && (
            <Stack spacing={2}>
              {challenge.hints.map((hint) => (
                <Group spacing="xs" align="flex-start" noWrap>
                  <Icon path={mdiLightbulbOnOutline} size={0.8} color={theme.colors.yellow[5]} />
                  <Text key={hint} size="sm" style={{ maxWidth: 'calc(100% - 2rem)' }}>
                    {hint}
                  </Text>
                </Group>
              ))}
            </Stack>
          )}
          {isDynamic && !challenge?.context?.instanceEntry && (
            <Group position="center" spacing={2}>
              <Button onClick={onCreateContainer} disabled={disabled} loading={disabled}>
                Start Instance
              </Button>
            </Group>
          )}
          {isDynamic && challenge?.context?.instanceEntry && (
            <Stack align="center">
              <Group>
                <Text size="sm" weight={600}>
                  Instance endpoint:
                  <Tooltip label="Click to copy" withArrow classNames={tooltipClasses}>
                    <Code
                      style={{
                        backgroundColor: 'transparent',
                        fontSize: theme.fontSizes.sm,
                        cursor: 'pointer',
                      }}
                      onClick={() => {
                        clipBoard.copy(challenge.context?.instanceEntry ?? '')
                        showNotification({
                          color: 'teal',
                          message: 'Instance endpoint has been copied to clipboard',
                          icon: <Icon path={mdiCheck} size={1} />,
                          disallowClose: true,
                        })
                      }}
                    >
                      {challenge?.context?.instanceEntry}
                    </Code>
                  </Tooltip>
                </Text>
                <Countdown time={challenge?.context?.closeTime ?? '0'} />
              </Group>
              <Group position="center">
                <Button color="orange" onClick={onProlongContainer} disabled={instanceLeft > 10}>
                  Extend Instance
                </Button>
                <Button color="red" onClick={onDestroyContainer} disabled={disabled}>
                  Destroy Instance
                </Button>
              </Group>
            </Stack>
          )}
        </Stack>
        <Divider />
        {solved ? (
          <Text align="center" weight={700}>
            This challenge has been solved
          </Text>
        ) : (
          <form onSubmit={onSubmit}>
            <TextInput
              placeholder={placeholder}
              value={flag}
              disabled={disabled}
              onChange={setFlag}
              styles={{
                rightSection: {
                  width: 'auto',
                },
                input: {
                  fontFamily: `${theme.fontFamilyMonospace}, ${theme.fontFamily}`,
                },
              }}
              rightSection={
                <Button type="submit" onClick={onSubmit} disabled={onSubmitting}>
                  Submit flag
                </Button>
              }
            />
          </form>
        )}
      </Stack>
    </Modal>
  )
}

export default ChallengeDetailModal
