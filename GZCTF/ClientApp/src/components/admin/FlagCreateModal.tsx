import { FC, useState } from 'react'
import { useParams } from 'react-router-dom'
import {
  Button,
  Group,
  Modal,
  ModalProps,
  Stack,
  TextInput,
  Text,
  useMantineTheme,
} from '@mantine/core'
import { useInputState } from '@mantine/hooks'
import { showNotification } from '@mantine/notifications'
import { mdiCheck } from '@mdi/js'
import { Icon } from '@mdi/react'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import api from '@Api'

const FlagCreateModal: FC<ModalProps> = (props) => {
  const [disabled, setDisabled] = useState(false)

  const { id, chalId } = useParams()
  const [numId, numCId] = [parseInt(id ?? '-1'), parseInt(chalId ?? '-1')]
  const theme = useMantineTheme()
  const [flag, setFlag] = useInputState('')

  const { data: challenge, mutate } = api.edit.useEditGetGameChallenge(numId, numCId, {
    refreshInterval: 0,
    revalidateIfStale: false,
    revalidateOnFocus: false,
  })

  const onCreate = () => {
    if (!flag) {
      return
    }

    setDisabled(true)
    api.edit
      .editAddFlags(numId, numCId, [
        {
          flag,
        },
      ])
      .then(() => {
        showNotification({
          color: 'teal',
          message: 'Flag created',
          icon: <Icon path={mdiCheck} size={1} />,
          disallowClose: true,
        })
        challenge &&
          mutate({
            ...challenge,
            flags: [...(challenge.flags ?? []), { flag }],
          })
      })
      .catch((err) => {
        showErrorNotification(err)
        setDisabled(false)
      })
      .finally(() => {
        setFlag('')
        setDisabled(false)
        props.onClose()
      })
  }

  return (
    <Modal {...props}>
      <Stack>
        <Text>Create a flag, each static challenge can have multiple flags, and any of them can be used to solve the challenge.</Text>
        <TextInput
          label="Flag"
          type="text"
          required
          placeholder="flag{...}"
          style={{ width: '100%' }}
          styles={{
            input: {
              fontFamily: theme.fontFamilyMonospace,
            },
          }}
          value={flag}
          onChange={setFlag}
        />
        <Group grow style={{ margin: 'auto', width: '100%' }}>
          <Button fullWidth disabled={disabled} onClick={onCreate}>
            Create flag
          </Button>
        </Group>
      </Stack>
    </Modal>
  )
}

export default FlagCreateModal
