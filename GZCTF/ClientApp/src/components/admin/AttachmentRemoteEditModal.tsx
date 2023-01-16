import { FC, useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { Button, Modal, ModalProps, Text, Stack, Textarea, useMantineTheme } from '@mantine/core'
import { showNotification } from '@mantine/notifications'
import { mdiCheck } from '@mdi/js'
import { Icon } from '@mdi/react'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import api, { FileType, FlagCreateModel } from '@Api'

const AttachmentRemoteEditModal: FC<ModalProps> = (props) => {
  const { id, chalId } = useParams()
  const [numId, numCId] = [parseInt(id ?? '-1'), parseInt(chalId ?? '-1')]

  const [disabled, setDisabled] = useState(false)

  const [text, setText] = useState('')
  const [flags, setFlags] = useState<FlagCreateModel[]>([])

  const theme = useMantineTheme()

  useEffect(() => {
    const list: FlagCreateModel[] = []
    text.split('\n').forEach((line) => {
      let part = line.split(' ')
      part = part.length === 1 ? line.split('\t') : part

      if (part.length !== 2) return

      list.push({
        flag: part[0],
        attachmentType: FileType.Remote,
        remoteUrl: part[1],
      })
    })
    setFlags(list)
  }, [text])

  const onUpload = () => {
    if (flags.length > 0) {
      api.edit
        .editAddFlags(numId, numCId, flags)
        .then(() => {
          showNotification({
            color: 'teal',
            message: 'Attachments updated',
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })
          props.onClose()
          api.edit.mutateEditGetGameChallenge(numId, numCId)
        })
        .catch((err) => showErrorNotification(err))
        .finally(() => {
          setDisabled(false)
        })
    }
  }

  return (
    <Modal {...props}>
      <Stack>
        <Text>
          Batch set remote attachments and corresponding flags, <strong> please separate the flag string and url with spaces or tabs </strong>
          , one group per line
        </Text>
        <Textarea
          styles={{
            input: {
              fontFamily: theme.fontFamilyMonospace,
            },
          }}
          placeholder={
            'flag{hello_world} http://example.com/1.zip\nflag{he11o_world} http://example.com/2.zip'
          }
          minRows={8}
          maxRows={12}
          value={text}
          onChange={(e) => setText(e.target.value)}
          required
        />
        <Button fullWidth disabled={disabled} onClick={onUpload}>
          Batch add
        </Button>
      </Stack>
    </Modal>
  )
}

export default AttachmentRemoteEditModal
