import { FC } from 'react'
import { Stack, Title, Text } from '@mantine/core'
import { useViewportSize } from '@mantine/hooks'
import IconWiderScreenRequired from './icon/WiderScreenRequiredIcon'

interface WithWiderScreenProps extends React.PropsWithChildren {
  minWidth?: number
}

const WithWiderScreen: FC<WithWiderScreenProps> = ({ children, minWidth = 1080 }) => {
  const view = useViewportSize()

  const tooSmall = minWidth > 0 && view.width > 0 && view.width < minWidth

  return tooSmall ? (
    <Stack spacing={0} align="center" justify="center" style={{ height: 'calc(100vh - 32px)' }}>
      <IconWiderScreenRequired />
      <Title order={1} color="#00bfa5" style={{ fontWeight: 'lighter' }}>
        Screen Width Insufficient
      </Title>
      <Text style={{ fontWeight: 'bold' }}>Please use a wider device to view this page</Text>
    </Stack>
  ) : (
    <>{children}</>
  )
}

export default WithWiderScreen
