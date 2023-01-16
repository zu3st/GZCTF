import { FC, useEffect, useRef } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { Text } from '@mantine/core'
import { showNotification } from '@mantine/notifications'
import { mdiCheck, mdiClose } from '@mdi/js'
import { Icon } from '@mdi/react'
import AccountView from '@Components/AccountView'
import { usePageTitle } from '@Utils/usePageTitle'
import api from '@Api'

const Confirm: FC = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const sp = new URLSearchParams(location.search)
  const token = sp.get('token')
  const email = sp.get('email')
  const runOnce = useRef(false)

  usePageTitle('Email Confirmation')

  useEffect(() => {
    if (token && email && !runOnce.current) {
      runOnce.current = true
      api.account
        .accountMailChangeConfirm({ token, email })
        .then(() => {
          showNotification({
            color: 'teal',
            title: 'Email verified',
            message: window.atob(email),
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })
        })
        .catch(() => {
          showNotification({
            color: 'red',
            title: 'Email verification failed',
            message: 'Invalid token or email',
            icon: <Icon path={mdiClose} size={1} />,
            disallowClose: true,
          })
        })
        .finally(() => {
          navigate('/')
        })
    }
  }, [])

  return (
    <AccountView>
      <Text>Verifying email...</Text>
    </AccountView>
  )
}

export default Confirm
