import { FC } from 'react'
import { useNavigate } from 'react-router-dom'
import { Button, Group, Modal, ModalProps, PasswordInput, Stack } from '@mantine/core'
import { useInputState } from '@mantine/hooks'
import { showNotification } from '@mantine/notifications'
import { mdiCheck, mdiClose } from '@mdi/js'
import { Icon } from '@mdi/react'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import api from '@Api'
import StrengthPasswordInput from './StrengthPasswordInput'

const PasswordChangeModal: FC<ModalProps> = (props) => {
  const [oldPwd, setOldPwd] = useInputState('')
  const [pwd, setPwd] = useInputState('')
  const [retypedPwd, setRetypedPwd] = useInputState('')

  const navigate = useNavigate()

  const onChangePwd = () => {
    if (!pwd || !retypedPwd) {
      showNotification({
        color: 'red',
        title: 'Password cannot be empty',
        message: 'Please check your input',
        icon: <Icon path={mdiClose} size={1} />,
        disallowClose: true,
      })
    } else if (pwd === retypedPwd) {
      api.account
        .accountChangePassword({
          old: oldPwd,
          new: pwd,
        })
        .then(() => {
          showNotification({
            color: 'teal',
            message: 'Password changed, please login again',
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })
          props.onClose()
          api.account.accountLogOut()
          navigate('/account/login')
        })
        .catch(showErrorNotification)
    } else {
      showNotification({
        color: 'red',
        title: 'Password mismatch',
        message: 'Please check your input',
        icon: <Icon path={mdiClose} size={1} />,
        disallowClose: true,
      })
    }
  }

  return (
    <Modal {...props}>
      <Stack>
        <PasswordInput
          required
          label="Old password"
          placeholder="P4ssW@rd"
          style={{ width: '100%' }}
          value={oldPwd}
          onChange={setOldPwd}
        />
        <StrengthPasswordInput value={pwd} onChange={setPwd} />
        <PasswordInput
          required
          label="Confirm password"
          placeholder="P4ssW@rd"
          style={{ width: '100%' }}
          value={retypedPwd}
          onChange={setRetypedPwd}
        />

        <Group position="right">
          <Button
            variant="default"
            onClick={() => {
              setOldPwd('')
              setPwd('')
              setRetypedPwd('')
              props.onClose()
            }}
          >
            Cancel
          </Button>
          <Button color="orange" onClick={onChangePwd}>
            Confirm
          </Button>
        </Group>
      </Stack>
    </Modal>
  )
}

export default PasswordChangeModal
