import { showNotification } from '@mantine/notifications'
import { mdiClose } from '@mdi/js'
import { Icon } from '@mdi/react'

export const showErrorNotification = (err: any) => {
  if (err?.response?.status === 429) {
    showNotification({
      color: 'red',
      message: 'Too many requests, please try again later',
      icon: <Icon path={mdiClose} size={1} />,
      disallowClose: true,
    })
    return
  }

  console.warn(err)
  showNotification({
    color: 'red',
    title: 'An error occurred',
    message: `${err?.response?.data?.title ?? err?.title ?? err ?? 'Unknown error'}`,
    icon: <Icon path={mdiClose} size={1} />,
    disallowClose: true,
  })
}
