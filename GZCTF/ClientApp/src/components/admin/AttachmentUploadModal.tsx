import { FC, useState } from 'react'
import { useParams } from 'react-router-dom'
import {
  Button,
  FileButton,
  Modal,
  ModalProps,
  Progress,
  Text,
  Stack,
  ScrollArea,
  Group,
  ActionIcon,
  Overlay,
  Center,
  Title,
  Card,
} from '@mantine/core'
import { showNotification } from '@mantine/notifications'
import { mdiCheck, mdiClose } from '@mdi/js'
import { Icon } from '@mdi/react'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import { useUploadStyles } from '@Utils/ThemeOverride'
import api, { FileType } from '@Api'

const AttachmentUploadModal: FC<ModalProps> = (props) => {
  const { id, chalId } = useParams()
  const [numId, numCId] = [parseInt(id ?? '-1'), parseInt(chalId ?? '-1')]
  const uploadFileName = `DYN_ATTACHMENT_${numCId}`
  const [disabled, setDisabled] = useState(false)

  const [progress, setProgress] = useState(0)
  const [files, setFiles] = useState<File[]>([])

  const { classes, theme } = useUploadStyles()

  const onUpload = () => {
    if (files.length > 0) {
      setProgress(0)
      setDisabled(true)

      api.assets
        .assetsUpload(
          {
            files,
          },
          { filename: uploadFileName },
          {
            onUploadProgress: (e) => {
              setProgress((e.loaded / (e.total ?? 1)) * 90)
            },
          }
        )
        .then((data) => {
          setProgress(95)
          if (data.data) {
            api.edit
              .editAddFlags(
                numId,
                numCId,
                data.data.map((f, idx) => ({
                  flag: files[idx].name,
                  attachmentType: FileType.Local,
                  fileHash: f.hash,
                }))
              )
              .then(() => {
                setProgress(0)
                showNotification({
                  color: 'teal',
                  message: 'Attachments updated',
                  icon: <Icon path={mdiCheck} size={1} />,
                  disallowClose: true,
                })
                setTimeout(() => {
                  api.edit.mutateEditGetGameChallenge(numId, numCId)
                }, 1000)
                props.onClose()
              })
              .catch((err) => showErrorNotification(err))
              .finally(() => {
                setDisabled(false)
              })
          }
        })
        .catch((err) => showErrorNotification(err))
        .finally(() => {
          setDisabled(false)
        })
    } else {
      showNotification({
        color: 'red',
        message: 'Please select at least one file',
        icon: <Icon path={mdiClose} size={1} />,
        disallowClose: true,
      })
    }
  }

  return (
    <Modal {...props}>
      <Stack>
        <Text>
          Batch upload dynamic attachments, <strong>need to be named after each flag</strong>
          , all attachments will be distributed with the same file name after uploading.
        </Text>
        <ScrollArea offsetScrollbars sx={{ height: '40vh', position: 'relative' }}>
          {files.length === 0 ? (
            <>
              <Overlay opacity={0.3} color={theme.colorScheme === 'dark' ? 'black' : 'white'} />
              <Center style={{ height: '40vh' }}>
                <Stack spacing={0}>
                  <Title order={2}>You haven't selected any files yet</Title>
                  <Text>The files you selected will be displayed here</Text>
                </Stack>
              </Center>
            </>
          ) : (
            <Stack spacing="xs">
              {files.map((file) => (
                <Card p={4}>
                  <Group position="apart">
                    <Text lineClamp={1} sx={{ fontFamily: theme.fontFamilyMonospace }}>
                      {file.name}
                    </Text>
                    <ActionIcon onClick={() => setFiles(files.filter((f) => f !== file))}>
                      <Icon path={mdiClose} size={1} />
                    </ActionIcon>
                  </Group>
                </Card>
              ))}
            </Stack>
          )}
        </ScrollArea>
        <Group grow>
          <FileButton multiple onChange={setFiles}>
            {(props) => (
              <Button {...props} disabled={disabled}>
                Select files
              </Button>
            )}
          </FileButton>
          <Button
            className={classes.uploadButton}
            disabled={disabled || files.length < 1}
            onClick={onUpload}
            color={progress !== 0 ? 'cyan' : theme.primaryColor}
          >
            <div className={classes.uploadLabel}>{progress !== 0 ? 'Uploading...' : 'Upload'}</div>
            {progress !== 0 && (
              <Progress
                value={progress}
                className={classes.uploadProgress}
                color={theme.fn.rgba(theme.colors[theme.primaryColor][2], 0.35)}
                radius="sm"
              />
            )}
          </Button>
        </Group>
      </Stack>
    </Modal>
  )
}

export default AttachmentUploadModal
