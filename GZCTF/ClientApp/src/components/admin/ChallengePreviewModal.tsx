import dayjs from 'dayjs'
import { FC, useEffect, useState } from 'react'
import React from 'react'
import {
  ActionIcon,
  Box,
  Button,
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
import { useDisclosure, useInputState } from '@mantine/hooks'
import { showNotification } from '@mantine/notifications'
import { mdiCheck, mdiDownload, mdiLightbulbOnOutline } from '@mdi/js'
import { Icon } from '@mdi/react'
import { Countdown, FlagPlaceholders } from '@Components/ChallengeDetailModal'
import MarkdownRender from '@Components/MarkdownRender'
import { ChallengeTagItemProps } from '@Utils/ChallengeItem'
import { useTooltipStyles } from '@Utils/ThemeOverride'
import { useTypographyStyles } from '@Utils/useTypographyStyles'
import { ChallengeType, ChallengeUpdateModel, FileType } from '@Api'

interface ChallengePreviewModalProps extends ModalProps {
  challenge: ChallengeUpdateModel
  type: ChallengeType
  attachmentType: FileType
  tagData: ChallengeTagItemProps
}

const ChallengePreviewModal: FC<ChallengePreviewModalProps> = (props) => {
  const { challenge, type, attachmentType, tagData, ...modalProps } = props
  const [downloadOpened, { close: downloadClose, open: downloadOpen }] = useDisclosure(false)

  const [placeholder, setPlaceholder] = useState('')
  const [flag, setFlag] = useInputState('')
  const [withContainer, setWithContainer] = useState(false)
  const [startTime, setStartTime] = useState(dayjs())
  const { classes: tooltipClasses } = useTooltipStyles()

  const onSubmit = (event: React.FormEvent) => {
    event.preventDefault()

    showNotification({
      color: 'teal',
      title: 'Flag seems to be submitted correctly!',
      message: flag,
      icon: <Icon path={mdiCheck} size={1} />,
      disallowClose: true,
    })
    setFlag('')
  }

  useEffect(() => {
    setPlaceholder(FlagPlaceholders[Math.floor(Math.random() * FlagPlaceholders.length)])
  }, [challenge])

  const isDynamic =
    type === ChallengeType.StaticContainer || type === ChallengeType.DynamicContainer
  const { classes, theme } = useTypographyStyles()

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
            <Title order={4}>{challenge?.title ?? ''}</Title>
          </Group>
          <Text weight={700} sx={(theme) => ({ fontFamily: theme.fontFamilyMonospace })}>
            {challenge?.originalScore ?? 500} pts
          </Text>
        </Group>
      }
    >
      <Stack spacing="sm" style={{ marginTop: theme.spacing.sm }}>
        <Divider />
        <Stack
          spacing="sm"
          justify="space-between"
          style={{ position: 'relative', minHeight: '20vh' }}
        >
          <LoadingOverlay visible={!challenge} />
          <Group grow noWrap position="right" align="flex-start" spacing={2}>
            <Box className={classes.root} style={{ minHeight: '4rem' }}>
              {attachmentType !== FileType.None && (
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
                      onClick={() =>
                        showNotification({
                          color: 'teal',
                          message: 'Attachment downloaded!',
                          icon: <Icon path={mdiCheck} size={1} />,
                          disallowClose: true,
                        })
                      }
                      style={{
                        float: 'right',
                      }}
                    >
                      <Icon path={mdiDownload} size={1} />
                    </ActionIcon>
                  </Popover.Target>
                  <Popover.Dropdown>
                    <Text size="sm" align="center">
                      Download attachment
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
          {isDynamic && !withContainer && (
            <Group position="center" spacing={2}>
              <Button
                onClick={() => {
                  setWithContainer(true)
                  setStartTime(dayjs())
                }}
              >
                Create Instance
              </Button>
            </Group>
          )}
          {isDynamic && withContainer && (
            <Stack align="center">
              <Group>
                <Text size="sm" weight={600}>
                  Instance Endpoint:
                  <Tooltip label="Click to copy" withArrow classNames={tooltipClasses}>
                    <Code
                      style={{
                        backgroundColor: 'transparent',
                        fontSize: theme.fontSizes.sm,
                        cursor: 'pointer',
                      }}
                    >
                      localhost:2333
                    </Code>
                  </Tooltip>
                </Text>
                <Countdown time={startTime.add(2, 'h').toJSON()} />
              </Group>
              <Group position="center">
                <Button color="orange" disabled>
                  Extend Instance
                </Button>
                <Button color="red" onClick={() => setWithContainer(false)}>
                  Destroy Instance
                </Button>
              </Group>
            </Stack>
          )}
        </Stack>
        <Divider />
        <form onSubmit={onSubmit}>
          <TextInput
            placeholder={placeholder}
            value={flag}
            onChange={setFlag}
            styles={{
              rightSection: {
                width: 'auto',
              },
              input: {
                fontFamily: `${theme.fontFamilyMonospace}, ${theme.fontFamily}`,
              },
            }}
            rightSection={<Button type="submit">Submit flag</Button>}
          />
        </form>
      </Stack>
    </Modal>
  )
}

export default ChallengePreviewModal
