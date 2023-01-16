import { FC, useState } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { Button, PasswordInput } from '@mantine/core'
import { getHotkeyHandler, useInputState } from '@mantine/hooks'
import { showNotification } from '@mantine/notifications'
import { mdiCheck, mdiClose } from '@mdi/js'
import { Icon } from '@mdi/react'
import AccountView from '@Components/AccountView'
import StrengthPasswordInput from '@Components/StrengthPasswordInput'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import { usePageTitle } from '@Utils/usePageTitle'
import api from '@Api'

const Reset: FC = () => {
  const location = useLocation()
  const sp = new URLSearchParams(location.search)
  const token = sp.get('token')
  const email = sp.get('email')
  const navigate = useNavigate()
  const [pwd, setPwd] = useInputState('')
  const [retypedPwd, setRetypedPwd] = useInputState('')
  const [disabled, setDisabled] = useState(false)

  usePageTitle('Reset Password')

  const onReset = () => {
    if (pwd !== retypedPwd) {
      showNotification({
        color: 'red',
        title: 'Password mismatch',
        message: 'Please retype your password',
        icon: <Icon path={mdiClose} size={1} />,
        disallowClose: true,
      })
      return
    }

    if (!(token && email)) {
      showNotification({
        color: 'red',
        title: 'Password reset failed',
        message: 'Invalid token or email',
        icon: <Icon path={mdiClose} size={1} />,
        disallowClose: true,
      })
      return
    }

    setDisabled(true)
    api.account
      .accountPasswordReset({
        rToken: token,
        email: email,
        password: pwd,
      })
      .then(() => {
        showNotification({
          color: 'teal',
          title: 'Password reset success',
          message: 'Redirecting to login page...',
          icon: <Icon path={mdiCheck} size={1} />,
          disallowClose: true,
        })
        navigate('/account/login')
      })
      .catch((err) => {
        showErrorNotification(err)
        setDisabled(false)
      })
  }

  const enterHandler = getHotkeyHandler([['Enter', onReset]])

  return (
    <AccountView>
      <StrengthPasswordInput
        value={pwd}
        onChange={(event) => setPwd(event.currentTarget.value)}
        label="New Password"
        disabled={disabled}
        onKeyDown={enterHandler}
      />
      <PasswordInput
        required
        value={retypedPwd}
        onChange={(event) => setRetypedPwd(event.currentTarget.value)}
        label="Retype new password"
        style={{ width: '100%' }}
        disabled={disabled}
        error={pwd !== retypedPwd}
        onKeyDown={enterHandler}
      />
      <Button fullWidth onClick={onReset} disabled={disabled}>
      Reset Password
      </Button>
    </AccountView>
  )
}

export default Reset
