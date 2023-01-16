import React, { FC, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Stack, Button, Text, Group, ScrollArea, Center, Title } from '@mantine/core'
import { useModals } from '@mantine/modals'
import { showNotification } from '@mantine/notifications'
import { mdiKeyboardBackspace, mdiCheck, mdiPlus } from '@mdi/js'
import { Icon } from '@mdi/react'
import GameNoticeEditCard from '@Components/admin/GameNoticeEditCard'
import GameNoticeEditModal from '@Components/admin/GameNoticeEditModal'
import WithGameTab from '@Components/admin/WithGameEditTab'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import api, { GameNotice } from '@Api'

const GameNoticeEdit: FC = () => {
  const { id } = useParams()
  const numId = parseInt(id ?? '-1')
  const { data: gameNotices, mutate } = api.edit.useEditGetGameNotices(numId, {
    refreshInterval: 0,
    revalidateIfStale: false,
    revalidateOnFocus: false,
  })

  const [isEditModalOpen, setIsEditModalOpen] = useState(false)
  const [activeGameNotice, setActiveGameNotice] = useState<GameNotice | null>(null)

  // delete
  const modals = useModals()
  const onDeleteGameNotice = (gameNotice: GameNotice) => {
    modals.openConfirmModal({
      title: 'Delete Notification',
      children: <Text> Are you sure you want to delete this notification? </Text>,
      onConfirm: () => onConfirmDelete(gameNotice),
      centered: true,
      labels: { confirm: 'Confirm', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
    })
  }
  const onConfirmDelete = (gameNotice: GameNotice) => {
    api.edit
      .editDeleteGameNotice(numId, gameNotice.id)
      .then(() => {
        showNotification({
          color: 'teal',
          message: 'Notice deleted',
          icon: <Icon path={mdiCheck} size={1} />,
          disallowClose: true,
        })
        mutate(gameNotices?.filter((t) => t.id !== gameNotice.id) ?? [])
      })
      .catch(showErrorNotification)
  }

  const navigate = useNavigate()
  return (
    <WithGameTab
      headProps={{ position: 'apart' }}
      head={
        <>
          <Button
            leftIcon={<Icon path={mdiKeyboardBackspace} size={1} />}
            onClick={() => navigate('/admin/games')}
          >
            Back
          </Button>

          <Group position="right">
            <Button
              leftIcon={<Icon path={mdiPlus} size={1} />}
              onClick={() => {
                setActiveGameNotice(null)
                setIsEditModalOpen(true)
              }}
            >
              Create Notification
            </Button>
          </Group>
        </>
      }
    >
      <ScrollArea style={{ height: 'calc(100vh-180px)', position: 'relative' }} offsetScrollbars>
        {!gameNotices || gameNotices?.length === 0 ? (
          <Center style={{ height: 'calc(100vh - 180px)' }}>
            <Stack spacing={0}>
              <Title order={2}>Ouch! This game has no notifications yet</Title>
              <Text>New notifications will be shown here</Text>
            </Stack>
          </Center>
        ) : (
          <Stack
            spacing="lg"
            align="center"
            style={{
              margin: '2%',
            }}
          >
            {gameNotices.map((gameNotice) => (
              <GameNoticeEditCard
                key={gameNotice.id}
                gameNotice={gameNotice}
                onDelete={() => {
                  onDeleteGameNotice(gameNotice)
                }}
                onEdit={() => {
                  setActiveGameNotice(gameNotice)
                  setIsEditModalOpen(true)
                }}
                style={{ width: '90%' }}
              />
            ))}
          </Stack>
        )}
      </ScrollArea>
      <GameNoticeEditModal
        centered
        size="30%"
        title={activeGameNotice ? 'Edit Notification' : 'Create Notification'}
        opened={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        gameNotice={activeGameNotice}
        mutateGameNotice={(gameNotice: GameNotice) => {
          mutate([gameNotice, ...(gameNotices?.filter((n) => n.id !== gameNotice.id) ?? [])])
        }}
      />
    </WithGameTab>
  )
}

export default GameNoticeEdit
