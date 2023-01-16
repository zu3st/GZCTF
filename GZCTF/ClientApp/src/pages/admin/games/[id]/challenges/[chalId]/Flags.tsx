import { FC, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import {
  Button,
  Chip,
  Divider,
  FileButton,
  Group,
  Input,
  Progress,
  Stack,
  TextInput,
  Text,
  Title,
  useMantineTheme,
  ScrollArea,
  Overlay,
  Center,
  Code,
} from '@mantine/core'
import { useModals } from '@mantine/modals'
import { showNotification } from '@mantine/notifications'
import { mdiCheck, mdiKeyboardBackspace, mdiPuzzleEditOutline } from '@mdi/js'
import { Icon } from '@mdi/react'
import AttachmentRemoteEditModal from '@Components/admin/AttachmentRemoteEditModal'
import AttachmentUploadModal from '@Components/admin/AttachmentUploadModal'
import FlagCreateModal from '@Components/admin/FlagCreateModal'
import FlagEditPanel from '@Components/admin/FlagEditPanel'
import WithGameEditTab from '@Components/admin/WithGameEditTab'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import { useUploadStyles } from '@Utils/ThemeOverride'
import api, { ChallengeType, FileType, FlagInfoModel } from '@Api'

const FileTypeDesrcMap = new Map<FileType, string>([
  [FileType.None, 'No Attachment'],
  [FileType.Remote, 'Remote File'],
  [FileType.Local, 'Local File'],
])

interface FlagEditProps {
  onDelete: (flag: FlagInfoModel) => void
}

// with only one attachment
const OneAttachmentWithFlags: FC<FlagEditProps> = ({ onDelete }) => {
  const { id, chalId } = useParams()
  const [numId, numCId] = [parseInt(id ?? '-1'), parseInt(chalId ?? '-1')]

  const { data: challenge, mutate } = api.edit.useEditGetGameChallenge(numId, numCId, {
    refreshInterval: 0,
    revalidateIfStale: false,
    revalidateOnFocus: false,
  })

  const [disabled, setDisabled] = useState(false)
  const [type, setType] = useState<FileType>(challenge?.attachment?.type ?? FileType.None)
  const [remoteUrl, setRemoteUrl] = useState(challenge?.attachment?.url ?? '')
  const [flagTemplate, setFlagTemplate] = useState(challenge?.flagTemplate ?? '')

  const modals = useModals()

  useEffect(() => {
    if (challenge) {
      setType(challenge.attachment?.type ?? FileType.None)
      setRemoteUrl(challenge.attachment?.url ?? '')
      setFlagTemplate(challenge.flagTemplate ?? '')
    }
  }, [challenge])

  const onConfirmClear = () => {
    setDisabled(true)
    api.edit
      .editUpdateAttachment(numId, numCId, { attachmentType: FileType.None })
      .then(() => {
        showNotification({
          color: 'teal',
          message: 'Attachment updated',
          icon: <Icon path={mdiCheck} size={1} />,
          disallowClose: true,
        })
        setType(FileType.None)
        challenge &&
          mutate({
            ...challenge,
            attachment: null,
          })
      })
      .catch((err) => showErrorNotification(err))
      .finally(() => {
        setDisabled(false)
      })
  }

  const { classes, theme } = useUploadStyles()
  const [progress, setProgress] = useState(0)
  const [flagCreateModalOpen, setFlagCreateModalOpen] = useState(false)

  const onUpload = (file: File) => {
    setProgress(0)
    setDisabled(true)

    api.assets
      .assetsUpload(
        {
          files: [file],
        },
        undefined,
        {
          onUploadProgress: (e) => {
            setProgress((e.loaded / (e.total ?? 1)) * 90)
          },
        }
      )
      .then((data) => {
        const file = data.data[0]
        setProgress(95)
        if (file) {
          api.edit
            .editUpdateAttachment(numId, numCId, {
              attachmentType: FileType.Local,
              fileHash: file.hash,
            })
            .then(() => {
              setProgress(0)
              setDisabled(false)
              mutate()
              showNotification({
                color: 'teal',
                message: 'Attachment updated',
                icon: <Icon path={mdiCheck} size={1} />,
                disallowClose: true,
              })
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
  }

  const onRemote = () => {
    if (remoteUrl.startsWith('http')) {
      setDisabled(true)
      api.edit
        .editUpdateAttachment(numId, numCId, {
          attachmentType: FileType.Remote,
          remoteUrl: remoteUrl,
        })
        .then(() => {
          showNotification({
            color: 'teal',
            message: 'Attachment updated',
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })
        })
        .catch((err) => showErrorNotification(err))
        .finally(() => {
          setDisabled(false)
        })
    }
  }

  const onChangeFlagTemplate = () => {
    if (flagTemplate !== challenge?.flagTemplate) {
      setDisabled(true)
      api.edit
        // allow empty flag template to be set (but not null or undefined)
        .editUpdateGameChallenge(numId, numCId, { flagTemplate })
        .then(() => {
          showNotification({
            color: 'teal',
            message: 'Flag template updated',
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })
          challenge && mutate({ ...challenge, flagTemplate: flagTemplate })
        })
        .catch(showErrorNotification)
        .finally(() => {
          setDisabled(false)
        })
    }
  }

  return (
    <Stack>
      <Group position="apart">
        <Title order={2}>Attachment Management</Title>
        {type !== FileType.Remote ? (
          <FileButton onChange={onUpload}>
            {(props) => (
              <Button
                {...props}
                fullWidth
                className={classes.uploadButton}
                disabled={type !== FileType.Local}
                style={{ width: '122px', marginTop: '24px' }}
                color={progress !== 0 ? 'cyan' : theme.primaryColor}
              >
                <div className={classes.uploadLabel}>{progress !== 0 ? 'Uploading' : 'Upload'}</div>
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
        ) : (
          <Button
            disabled={disabled}
            style={{ width: '122px', marginTop: '24px' }}
            onClick={onRemote}
          >
            Save URL
          </Button>
        )}
      </Group>
      <Divider />
      <Group position="apart">
        <Input.Wrapper label="Attachment Type" required>
          <Chip.Group
            mt={8}
            value={type}
            onChange={(e) => {
              if (e === FileType.None) {
                modals.openConfirmModal({
                  title: 'Clear Attachment',
                  children: <Text size="sm">Are you sure to clear the attachment?</Text>,
                  onConfirm: onConfirmClear,
                  centered: true,
                  labels: { confirm: 'Confirm', cancel: 'Cancel' },
                  confirmProps: { color: 'orange' },
                })
              } else {
                setType(e as FileType)
              }
            }}
          >
            {Object.entries(FileType).map((type) => (
              <Chip key={type[0]} value={type[1]}>
                {FileTypeDesrcMap.get(type[1])}
              </Chip>
            ))}
          </Chip.Group>
        </Input.Wrapper>
        {type !== FileType.Remote ? (
          <TextInput
            label="Attachment URL"
            readOnly
            disabled={disabled || type === FileType.None}
            value={challenge?.attachment?.url ?? ''}
            style={{ width: 'calc(100% - 320px)' }}
            onClick={() => challenge?.attachment?.url && window.open(challenge?.attachment?.url)}
          />
        ) : (
          <TextInput
            label="Attachment URL"
            disabled={disabled}
            value={remoteUrl}
            style={{ width: 'calc(100% - 320px)' }}
            onChange={(e) => setRemoteUrl(e.target.value)}
          />
        )}
      </Group>
      <Group position="apart" mt={20}>
        <Title order={2}>Flag Management</Title>
        {challenge?.type === ChallengeType.DynamicContainer ? (
          <Button disabled={disabled} onClick={onChangeFlagTemplate}>
            Save flag template
          </Button>
        ) : (
          <Button
            disabled={disabled}
            style={{ width: '122px' }}
            onClick={() => setFlagCreateModalOpen(true)}
          >
            Add flag
          </Button>
        )}
      </Group>
      <Divider />
      {challenge?.type === ChallengeType.DynamicContainer ? (
        <Stack>
          <TextInput
            label="Flag Template"
            value={flagTemplate}
            placeholder="flag{random_uuid}"
            onChange={(e) => setFlagTemplate(e.target.value)}
            styles={{
              input: {
                fontFamily: theme.fontFamilyMonospace,
              },
            }}
          />
          <Stack spacing={6} pb={8}>
          <Text size="xs">Please enter the flag template, if empty a random UID will be used.</Text>
            <Text size="xs">
              If <Code>[TEAM_HASH]</Code> is specified, it will be automatically replaced by the hash value
              generated by the team Token and related information
            </Text>
            <Text size="xs">
              If <Code>[TEAM_HASH]</Code> is not specified, Leet mode will be enabled,
              randomly transforming the template
            </Text>
            <Text size="xs">
              To use both <Code>[TEAM_HASH]</Code> and Leet mode, add <Code>[LEET]</Code>
              <Text span weight={700}>before</Text> the flag template
            </Text>
          </Stack>
        </Stack>
      ) : (
        <ScrollArea sx={{ height: 'calc(100vh - 430px)', position: 'relative' }}>
          {!challenge?.flags.length && (
            <>
              <Overlay opacity={0.3} color={theme.colorScheme === 'dark' ? 'black' : 'white'} />
              <Center style={{ height: 'calc(100vh - 430px)' }}>
                <Stack spacing={0}>
                  <Title order={2}>Flag list is empty</Title>
                  <Text>Please add flags through the upper right corner</Text>
                </Stack>
              </Center>
            </>
          )}
          <FlagEditPanel
            flags={challenge?.flags}
            onDelete={onDelete}
            unifiedAttachment={challenge?.attachment}
          />
        </ScrollArea>
      )}
      <FlagCreateModal
        title="Add flag"
        centered
        opened={flagCreateModalOpen}
        onClose={() => setFlagCreateModalOpen(false)}
      />
    </Stack>
  )
}

const FlagsWithAttachments: FC<FlagEditProps> = ({ onDelete }) => {
  const { id, chalId } = useParams()
  const [numId, numCId] = [parseInt(id ?? '-1'), parseInt(chalId ?? '-1')]

  const theme = useMantineTheme()

  const { data: challenge } = api.edit.useEditGetGameChallenge(numId, numCId, {
    refreshInterval: 0,
    revalidateIfStale: false,
    revalidateOnFocus: false,
  })

  const [attachmentUploadModalOpened, setAttachmentUploadModalOpened] = useState(false)
  const [remoteAttachmentModalOpened, setRemoteAttachmentModalOpened] = useState(false)

  return (
    <Stack>
      <Group position="apart" mt={20}>
        <Title order={2}>Flags</Title>
        <Group position="right">
          <Button onClick={() => setRemoteAttachmentModalOpened(true)}>Add remote attachment</Button>
          <Button onClick={() => setAttachmentUploadModalOpened(true)}>Upload dynamic attachment</Button>
        </Group>
      </Group>
      <Divider />
      <ScrollArea sx={{ height: 'calc(100vh - 250px)', position: 'relative' }}>
        {!challenge?.flags.length && (
          <>
            <Overlay opacity={0.3} color={theme.colorScheme === 'dark' ? 'black' : 'white'} />
            <Center style={{ height: 'calc(100vh - 250px)' }}>
              <Stack spacing={0}>
                <Title order={2}>Flag list is empty</Title>
                <Text>Please add flags through the upper right corner</Text>
              </Stack>
            </Center>
          </>
        )}
        <FlagEditPanel flags={challenge?.flags} onDelete={onDelete} />
      </ScrollArea>
      <AttachmentUploadModal
        title="Batch add dynamic attachments"
        size="40%"
        centered
        opened={attachmentUploadModalOpened}
        onClose={() => setAttachmentUploadModalOpened(false)}
      />
      <AttachmentRemoteEditModal
        title="Batch add remote attachments"
        size="40%"
        centered
        opened={remoteAttachmentModalOpened}
        onClose={() => setRemoteAttachmentModalOpened(false)}
      />
    </Stack>
  )
}

const GameChallengeEdit: FC = () => {
  const navigate = useNavigate()
  const { id, chalId } = useParams()
  const [numId, numCId] = [parseInt(id ?? '-1'), parseInt(chalId ?? '-1')]

  const theme = useMantineTheme()
  const modals = useModals()

  const { data: challenge, mutate } = api.edit.useEditGetGameChallenge(numId, numCId, {
    refreshInterval: 0,
    revalidateIfStale: false,
    revalidateOnFocus: false,
  })

  const onDeleteFlag = (flag: FlagInfoModel) => {
    modals.openConfirmModal({
      title: 'Delete flag',
      size: '35%',
      children: (
        <Stack>
          <Text>Are you sure to delete the following flag?</Text>
          <Text style={{ fontFamily: theme.fontFamilyMonospace }}>{flag.flag}</Text>
        </Stack>
      ),
      onConfirm: () => flag.id && onConfirmDeleteFlag(flag.id),
      centered: true,
      labels: { confirm: 'Confirm', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
    })
  }

  const onConfirmDeleteFlag = (id: number) => {
    api.edit
      .editRemoveFlag(numId, numCId, id)
      .then(() => {
        showNotification({
          color: 'teal',
          message: 'Flag deleted',
          icon: <Icon path={mdiCheck} size={1} />,
          disallowClose: true,
        })
        challenge &&
          mutate({
            ...challenge,
            flags: challenge.flags.filter((f) => f.id !== id),
          })
      })
      .catch(showErrorNotification)
  }

  return (
    <WithGameEditTab
      isLoading={!challenge}
      headProps={{ position: 'apart' }}
      head={
        <>
          <Group noWrap position="left">
            <Button
              leftIcon={<Icon path={mdiKeyboardBackspace} size={1} />}
              onClick={() => navigate(`/admin/games/${id}/challenges`)}
            >
              Back
            </Button>
            <Title lineClamp={1} style={{ wordBreak: 'break-all' }}>
              # {challenge?.title}
            </Title>
          </Group>
          <Group noWrap position="right">
            <Button
              leftIcon={<Icon path={mdiPuzzleEditOutline} size={1} />}
              onClick={() => navigate(`/admin/games/${id}/challenges/${numCId}`)}
            >
              Edit challenge
            </Button>
          </Group>
        </>
      }
    >
      {challenge && challenge.type === ChallengeType.DynamicAttachment ? (
        <FlagsWithAttachments onDelete={onDeleteFlag} />
      ) : (
        <OneAttachmentWithFlags onDelete={onDeleteFlag} />
      )}
    </WithGameEditTab>
  )
}

export default GameChallengeEdit
