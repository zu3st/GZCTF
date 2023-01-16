import { FC, useEffect } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { Stack, Title, Text } from '@mantine/core'
import WithNavBar from '@Components/WithNavbar'
import Icon404 from '@Components/icon/404Icon'
import { usePageTitle } from '@Utils/usePageTitle'

const Error404: FC = () => {
  const navigate = useNavigate()
  const location = useLocation()

  usePageTitle('The Nowhere')

  useEffect(() => {
    if (location.pathname !== '/404') {
      navigate('/404')
    }
  }, [location])

  return (
    <WithNavBar minWidth={0}>
      <Stack spacing={0} align="center" justify="center" style={{ height: 'calc(100vh - 32px)' }}>
        <Icon404 />
        <Title order={1} color="#00bfa5" style={{ fontWeight: 'lighter' }}>
        Page not found
        </Title>
        <Text style={{ fontWeight: 'bold' }}>What are you looking for?</Text>
      </Stack>
    </WithNavBar>
  )
}

export default Error404
