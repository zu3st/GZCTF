import dayjs from 'dayjs'
import { FC, useEffect, useState } from 'react'
import {
  Button,
  Card,
  Divider,
  FileButton,
  Group,
  List,
  Modal,
  ModalProps,
  Progress,
  Stack,
  Text,
  Title,
  useMantineTheme,
} from '@mantine/core'
import { showNotification } from '@mantine/notifications'
import { mdiCheck, mdiExclamationThick, mdiFileDocumentOutline, mdiFileHidden } from '@mdi/js'
import { Icon } from '@mdi/react'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import { useUploadStyles } from '@Utils/ThemeOverride'
import api from '@Api'
import MarkdownRender from './MarkdownRender'

interface WriteupSubmitModalProps extends ModalProps {
  gameId: number
  wpddl: string
}

export const WriteupSubmitModal: FC<WriteupSubmitModalProps> = ({ gameId, wpddl, ...props }) => {
  const { data, mutate } = api.game.useGameGetWriteup(gameId, {
    refreshInterval: 0,
    revalidateOnFocus: false,
  })

  const theme = useMantineTheme()
  const { classes } = useUploadStyles()
  const [ddl, setDdl] = useState(dayjs(wpddl))
  const [disabled, setDisabled] = useState(dayjs().isAfter(wpddl))
  const [progress, setProgress] = useState(0)
  const noteColor = data?.submitted ? theme.colors.teal[5] : theme.colors.red[5]

  useEffect(() => {
    setDdl(dayjs(wpddl))
    setDisabled(dayjs().isAfter(wpddl))
  }, [wpddl])

  const onUpload = (file: File) => {
    setProgress(0)
    setDisabled(true)

    api.game
      .gameSubmitWriteup(
        gameId,
        {
          file,
        },
        {
          onUploadProgress: (e) => {
            setProgress((e.loaded / (e.total ?? 1)) * 100)
          },
        }
      )
      .then(() => {
        setProgress(100)
        showNotification({
          color: 'teal',
          message: 'Writeup submitted',
          icon: <Icon path={mdiCheck} size={1} />,
          disallowClose: true,
        })
        mutate()
        setDisabled(false)
      })
      .catch((err) => showErrorNotification(err))
      .finally(() => {
        setProgress(0)
        setDisabled(false)
      })
  }

  const hunamize = (size: number) => {
    if (size < 1024) {
      return `${size} B`
    } else if (size < 1024 * 1024) {
      return `${(size / 1024).toFixed(2)} KB`
    } else if (size < 1024 * 1024 * 1024) {
      return `${(size / 1024 / 1024).toFixed(2)} MB`
    } else {
      return `${(size / 1024 / 1024 / 1024).toFixed(2)} GB`
    }
  }

  return (
    <Modal
      title={
        <Group style={{ width: '100%' }} position="apart">
          <Title order={4}>Manage Writeups</Title>
          <Group spacing={4}>
            <Icon
              path={data?.submitted ? mdiCheck : mdiExclamationThick}
              size={0.9}
              color={noteColor}
            />
            <Text weight={600} size="md" color={noteColor}>
              {data?.submitted ? 'Submitted' : 'Not Submitted'}
            </Text>
          </Group>
        </Group>
      }
      {...props}
      styles={{
        ...props.styles,
        header: {
          margin: 0,
        },
        title: {
          width: '100%',
          margin: 0,
        },
      }}
    >
      <Stack spacing="xs" mt="sm">
        <Divider />
        <Title order={5}>Submission Instructions</Title>
        <List
          styles={{
            itemWrapper: {
              maxWidth: 'calc(100% - 2rem)',
            },
          }}
        >
          <List.Item>
            <Text>
              Please submit your writeup before
              <Text mx={5} span weight={600} color="yellow">
                {ddl.format('YYYY-MM-DD HH:mm:ss')}
              </Text>
              , failure to do so will be considered as a game forfeit.
            </Text>
          </List.Item>
          <List.Item>
            <Text>
              Please organize all solved challenges into
              <Text mx={5} weight={600} span color="yellow">
                a single PDF document
              </Text>
              , in addition to the solution, a screenshot of the relevant information when obtaining each problem's
              <Text mx={5} span style={{ fontFamily: theme.fontFamilyMonospace }}>
                flag
              </Text>
              string is also required.
            </Text>
          </List.Item>
          <List.Item>
            <Text>
              Please upload a PDF document smaller than
              <Text mx={5} weight={600} span color="yellow">
                20MB
              </Text>
              .
            </Text>
          </List.Item>
        </List>
        {data?.note && (
          <>
            <Title order={5}>Additional Notes</Title>
            <MarkdownRender source={data.note} />
          </>
        )}
        <Title order={5}>Current Submission</Title>
        <Card>
          {data?.submitted ? (
            <Group>
              <Icon path={mdiFileDocumentOutline} size={1.5} />
              <Stack spacing={0}>
                <Text weight={600} size="md">
                  {data?.name ?? 'Writeup-1-2-2022-10-11T12:00:00.pdf'}
                </Text>
                <Text
                  weight={600}
                  size="sm"
                  color="dimmed"
                  style={{ fontFamily: theme.fontFamilyMonospace }}
                >
                  {data?.fileSize ? hunamize(data.fileSize) : '456.64 KB'}
                </Text>
              </Stack>
            </Group>
          ) : (
            <Group>
              <Icon path={mdiFileHidden} size={1.5} />
              <Stack spacing={0}>
                <Text weight={600} size="md">
                  Your team has not submitted a writeup yet
                </Text>
              </Stack>
            </Group>
          )}
        </Card>
        <FileButton onChange={onUpload} accept="application/pdf">
          {(props) => (
            <Button
              {...props}
              fullWidth
              className={classes.uploadButton}
              disabled={disabled}
              color={progress !== 0 ? 'cyan' : theme.primaryColor}
            >
              <div className={classes.uploadLabel}>
                {dayjs().isAfter(ddl)
                  ? 'Deadline has passed'
                  : progress !== 0
                  ? 'Uploading'
                  : 'Upload Writeup'}
              </div>
              {progress !== 0 && (
                <Progress
                  value={progress}
                  className={classes.uploadProgress}
                  color={theme.fn.rgba(theme.colors[theme.primaryColor][2], 0.35)}
                  radius="sm"
                />
              )}
            </Button>
          )}
        </FileButton>
      </Stack>
    </Modal>
  )
}

export default WriteupSubmitModal
