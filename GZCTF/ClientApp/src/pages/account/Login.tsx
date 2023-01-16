import { FC, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { PasswordInput, Grid, TextInput, Button, Anchor } from '@mantine/core'
import { useInputState, getHotkeyHandler } from '@mantine/hooks'
import { showNotification } from '@mantine/notifications'
import { mdiCheck, mdiClose } from '@mdi/js'
import { Icon } from '@mdi/react'
import AccountView from '@Components/AccountView'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import { usePageTitle } from '@Utils/usePageTitle'
import api from '@Api'

const Login: FC = () => {
  const params = useParams()
  const navigate = useNavigate()

  const [pwd, setPwd] = useInputState('')
  const [uname, setUname] = useInputState('')
  const [disabled, setDisabled] = useState(false)

  usePageTitle('Login')

  const onLogin = () => {
    setDisabled(true)

    if (uname.length === 0 || pwd.length < 6) {
      showNotification({
        color: 'red',
        title: 'Please check your input',
        message: 'Invalid username or password',
        icon: <Icon path={mdiClose} size={1} />,
        disallowClose: true,
      })
      setDisabled(false)
      return
    }

    api.account
      .accountLogIn({
        userName: uname,
        password: pwd,
      })
      .then(() => {
        showNotification({
          color: 'teal',
          title: 'Login success',
          message: 'Redirecting to the previous page',
          icon: <Icon path={mdiCheck} size={1} />,
          disallowClose: true,
        })
        api.account.mutateAccountProfile()
        const from = params['from']
        navigate(from ? (from as string) : '/')
      })
      .catch((err) => {
        showErrorNotification(err)
        setDisabled(false)
      })
  }

  const enterHandler = getHotkeyHandler([['Enter', onLogin]])

  return (
    <AccountView>
      <TextInput
        required
        label="Username or Email"
        placeholder="ctfer"
        type="text"
        style={{ width: '100%' }}
        value={uname}
        disabled={disabled}
        onChange={(event) => setUname(event.currentTarget.value)}
        onKeyDown={enterHandler}
      />
      <PasswordInput
        required
        label="Password"
        id="your-password"
        placeholder="P4ssW@rd"
        style={{ width: '100%' }}
        value={pwd}
        disabled={disabled}
        onChange={(event) => setPwd(event.currentTarget.value)}
        onKeyDown={enterHandler}
      />
      <Anchor
        sx={(theme) => ({
          fontSize: theme.fontSizes.xs,
          alignSelf: 'end',
        })}
        component={Link}
        to="/account/recovery"
      >
        Forgot password?
      </Anchor>
      <Grid grow style={{ width: '100%' }}>
        <Grid.Col span={2}>
          <Button fullWidth variant="outline" component={Link} to="/account/register">
          Register
          </Button>
        </Grid.Col>
        <Grid.Col span={2}>
          <Button fullWidth disabled={disabled} onClick={onLogin}>
          Log in
          </Button>
        </Grid.Col>
      </Grid>
    </AccountView>
  )
}

export default Login
