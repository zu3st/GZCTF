import { FC, useState } from 'react'
import { Link } from 'react-router-dom'
import { TextInput, Button, Anchor } from '@mantine/core'
import { useInputState } from '@mantine/hooks'
import { showNotification, updateNotification } from '@mantine/notifications'
import { mdiCheck, mdiClose } from '@mdi/js'
import { Icon } from '@mdi/react'
import AccountView from '@Components/AccountView'
import { usePageTitle } from '@Utils/usePageTitle'
import { useReCaptcha } from '@Utils/useRecaptcha'
import api from '@Api'

const Recovery: FC = () => {
  const [email, setEmail] = useInputState('')
  const reCaptcha = useReCaptcha('recovery')
  const [disabled, setDisabled] = useState(false)

  usePageTitle('Account Recovery')

  const onRecovery = async (event: React.FormEvent) => {
    event.preventDefault()

    const token = await reCaptcha?.getToken()

    if (!token) {
      showNotification({
        color: 'orange',
        title: 'Are you a robot?',
        message: 'Please wait for the CAPTCHA to load...',
        loading: true,
        disallowClose: true,
      })
      return
    }

    setDisabled(true)

    showNotification({
      color: 'orange',
      id: 'recovery-status',
      title: 'Request sent',
      message: 'We are processing your request...',
      loading: true,
      autoClose: false,
      disallowClose: true,
    })

    api.account
      .accountRecovery({
        email,
        gToken: token,
      })
      .then(() => {
        updateNotification({
          id: 'recovery-status',
          color: 'teal',
          title: 'A recovery email has been sent',
          message: 'Please check your inbox and spam folder',
          icon: <Icon path={mdiCheck} size={1} />,
          disallowClose: true,
        })
      })
      .catch((err) => {
        updateNotification({
          id: 'recovery-status',
          color: 'red',
          title: 'An error occurred',
          message: `${err.response.data.title}`,
          icon: <Icon path={mdiClose} size={1} />,
        })
      })
      .finally(() => {
        setDisabled(false)
      })
  }

  return (
    <AccountView onSubmit={onRecovery}>
      <TextInput
        required
        label="Email"
        placeholder="ctf@example.com"
        type="email"
        style={{ width: '100%' }}
        value={email}
        disabled={disabled}
        onChange={(event) => setEmail(event.currentTarget.value)}
      />
      <Anchor
        sx={(theme) => ({
          fontSize: theme.fontSizes.xs,
          alignSelf: 'end',
        })}
        component={Link}
        to="/account/login"
      >
        Back to Login
      </Anchor>
      <Button disabled={disabled} fullWidth onClick={onRecovery}>
        Send Recovery Email
      </Button>
    </AccountView>
  )
}

export default Recovery
