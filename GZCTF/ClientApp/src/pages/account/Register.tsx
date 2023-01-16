import { FC, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Button, Anchor, TextInput, PasswordInput } from '@mantine/core'
import { useInputState } from '@mantine/hooks'
import { showNotification, updateNotification } from '@mantine/notifications'
import { mdiCheck, mdiClose } from '@mdi/js'
import { Icon } from '@mdi/react'
import AccountView from '@Components/AccountView'
import StrengthPasswordInput from '@Components/StrengthPasswordInput'
import { usePageTitle } from '@Utils/usePageTitle'
import { useReCaptcha } from '@Utils/useRecaptcha'
import api, { RegisterStatus } from '@Api'

const RegisterStatusMap = new Map([
  [
    RegisterStatus.LoggedIn,
    {
      message: 'Registration success',
    },
  ],
  [
    RegisterStatus.AdminConfirmationRequired,
    {
      title: 'Registration request sent',
      message: 'Please wait for the administrator to review and activate your account',
    },
  ],
  [
    RegisterStatus.EmailConfirmationRequired,
    {
      title: 'A registration email has been sent',
      message: 'Please check your inbox and spam folder',
    },
  ],
  [undefined, undefined],
])

const Register: FC = () => {
  const [pwd, setPwd] = useInputState('')
  const [retypedPwd, setRetypedPwd] = useInputState('')
  const [uname, setUname] = useInputState('')
  const [email, setEmail] = useInputState('')
  const [disabled, setDisabled] = useState(false)

  const navigate = useNavigate()
  const reCaptcha = useReCaptcha('register')

  usePageTitle('Registration')

  const onRegister = async (event: React.FormEvent) => {
    event.preventDefault()

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
      id: 'register-status',
      title: 'Registering',
      message: 'We are processing your request...',
      loading: true,
      autoClose: false,
      disallowClose: true,
    })

    api.account
      .accountRegister({
        userName: uname,
        password: pwd,
        email: email,
        gToken: token,
      })
      .then((res) => {
        const data = RegisterStatusMap.get(res.data.data)
        if (data) {
          updateNotification({
            id: 'register-status',
            color: 'teal',
            title: data.title,
            message: data.message,
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })

          if (res.data.data === RegisterStatus.LoggedIn) navigate('/')
          else navigate('/account/login')
        }
      })
      .catch((err) => {
        updateNotification({
          id: 'register-status',
          color: 'red',
          title: 'Something went wrong',
          message: `${err.response.data.title}`,
          icon: <Icon path={mdiClose} size={1} />,
          disallowClose: true,
        })
      })
      .finally(() => {
        setDisabled(false)
      })
  }

  return (
    <AccountView onSubmit={onRegister}>
      <TextInput
        required
        label="Email"
        type="email"
        placeholder="ctf@example.com"
        style={{ width: '100%' }}
        value={email}
        disabled={disabled}
        onChange={(event) => setEmail(event.currentTarget.value)}
      />
      <TextInput
        required
        label="Username"
        type="text"
        placeholder="ctfer"
        style={{ width: '100%' }}
        value={uname}
        disabled={disabled}
        onChange={(event) => setUname(event.currentTarget.value)}
      />
      <StrengthPasswordInput
        value={pwd}
        onChange={(event) => setPwd(event.currentTarget.value)}
        disabled={disabled}
      />
      <PasswordInput
        required
        value={retypedPwd}
        onChange={(event) => setRetypedPwd(event.currentTarget.value)}
        disabled={disabled}
        label="Retype password"
        style={{ width: '100%' }}
        error={pwd !== retypedPwd}
      />
      <Anchor
        sx={(theme) => ({
          fontSize: theme.fontSizes.xs,
          alignSelf: 'end',
        })}
        component={Link}
        to="/account/login"
      >
        Already have an account?
      </Anchor>
      <Button type="submit" fullWidth onClick={onRegister} disabled={disabled}>
      Register
      </Button>
    </AccountView>
  )
}

export default Register
