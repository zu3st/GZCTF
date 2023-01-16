import React, { FC, useEffect, useState } from 'react'
import {
  Group,
  Table,
  Text,
  ActionIcon,
  Badge,
  Avatar,
  TextInput,
  Paper,
  ScrollArea,
  Switch,
  Stack,
  Button,
  Code,
} from '@mantine/core'
import { useClipboard, useInputState } from '@mantine/hooks'
import { useModals } from '@mantine/modals'
import { showNotification } from '@mantine/notifications'
import {
  mdiArrowLeftBold,
  mdiArrowRightBold,
  mdiCheck,
  mdiDeleteOutline,
  mdiLockReset,
  mdiMagnify,
  mdiPencilOutline,
} from '@mdi/js'
import { Icon } from '@mdi/react'
import { ActionIconWithConfirm } from '@Components/ActionIconWithConfirm'
import AdminPage from '@Components/admin/AdminPage'
import UserEditModal, { RoleColorMap } from '@Components/admin/UserEditModal'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import { useTableStyles } from '@Utils/ThemeOverride'
import { useArrayResponse } from '@Utils/useArrayResponse'
import { useUser } from '@Utils/useUser'
import api, { Role, UserInfoModel } from '@Api'

const ITEM_COUNT_PER_PAGE = 30

const Users: FC = () => {
  const [page, setPage] = useState(1)
  const [update, setUpdate] = useState(new Date())
  const [isEditModalOpen, setIsEditModalOpen] = useState(false)
  const [activeUser, setActiveUser] = useState<UserInfoModel>({})
  const {
    data: users,
    total,
    setData: setUsers,
    updateData: updateUsers,
  } = useArrayResponse<UserInfoModel>()
  const [hint, setHint] = useInputState('')
  const [searching, setSearching] = useState(false)
  const [disabled, setDisabled] = useState(false)
  const [current, setCurrent] = useState(0)

  const modals = useModals()
  const { user: currentUser } = useUser()
  const clipboard = useClipboard()
  const { classes, theme } = useTableStyles()

  useEffect(() => {
    api.admin
      .adminUsers({
        count: ITEM_COUNT_PER_PAGE,
        skip: (page - 1) * ITEM_COUNT_PER_PAGE,
      })
      .then((res) => {
        setUsers(res.data)
        setCurrent((page - 1) * ITEM_COUNT_PER_PAGE + res.data.length)
      })
  }, [page, update])

  const onSearch = () => {
    if (!hint) {
      api.admin
        .adminUsers({
          count: ITEM_COUNT_PER_PAGE,
          skip: (page - 1) * ITEM_COUNT_PER_PAGE,
        })
        .then((res) => {
          setUsers(res.data)
          setCurrent((page - 1) * ITEM_COUNT_PER_PAGE + res.data.length)
        })
      return
    }

    setSearching(true)

    api.admin
      .adminSearchUsers({
        hint,
      })
      .then((res) => {
        setUsers(res.data)
        setCurrent((page - 1) * ITEM_COUNT_PER_PAGE + res.data.length)
      })
      .catch(showErrorNotification)
      .finally(() => {
        setSearching(false)
      })
  }

  const onToggleActive = (user: UserInfoModel) => {
    setDisabled(true)
    api.admin
      .adminUpdateUserInfo(user.id!, {
        emailConfirmed: !user.emailConfirmed,
      })
      .then(() => {
        users &&
          updateUsers(
            users.map((u) =>
              u.id === user.id
                ? {
                    ...u,
                    emailConfirmed: !u.emailConfirmed,
                  }
                : u
            )
          )
      })
      .catch(showErrorNotification)
      .finally(() => {
        setDisabled(false)
      })
  }

  const onResetPassword = async (user: UserInfoModel) => {
    setDisabled(true)
    try {
      const res = await api.admin.adminResetPassword(user.id!)

      modals.openModal({
        title: `Reset password for ${user.userName}`,
        centered: true,
        withCloseButton: false,
        children: (
          <Stack>
            <Text>
              The user's password has been reset,
              <Text span weight={700}>
                this password will only be displayed once
              </Text>
              .
            </Text>
            <Text
              weight={700}
              align="center"
              sx={(theme) => ({ fontFamily: theme.fontFamilyMonospace })}
            >
              {res.data}
            </Text>
            <Button
              onClick={() => {
                clipboard.copy(res.data)
                showNotification({
                  message: 'Password has been copied to clipboard',
                  color: 'teal',
                  icon: <Icon path={mdiCheck} size={1} />,
                  disallowClose: true,
                })
              }}
            >
              Copy to clipboard
            </Button>
          </Stack>
        ),
      })
    } catch (err: any) {
      showErrorNotification(err)
    } finally {
      setDisabled(false)
    }
  }

  const onDelete = async (user: UserInfoModel) => {
    try {
      setDisabled(true)
      if (!user.id) return

      await api.admin.adminDeleteUser(user.id)
      showNotification({
        message: `${user.userName} has been deleted`,
        color: 'teal',
        icon: <Icon path={mdiCheck} size={1} />,
        disallowClose: true,
      })
      users && updateUsers(users.filter((x) => x.id !== user.id))
      setCurrent(current - 1)
      setUpdate(new Date())
    } catch (e: any) {
      showErrorNotification(e)
    } finally {
      setDisabled(false)
    }
  }

  return (
    <AdminPage
      isLoading={searching || !users}
      head={
        <>
          <TextInput
            icon={<Icon path={mdiMagnify} size={1} />}
            style={{ width: '30%' }}
            placeholder="Search by user ID/username/email/student ID/name"
            value={hint}
            onChange={setHint}
            onKeyDown={(e) => {
              !searching && e.key === 'Enter' && onSearch()
            }}
          />
          <Group position="right">
            <Text weight="bold" size="sm">
              Showing <Code>{current}</Code> / <Code>{total}</Code> users
            </Text>
            <ActionIcon size="lg" disabled={page <= 1} onClick={() => setPage(page - 1)}>
              <Icon path={mdiArrowLeftBold} size={1} />
            </ActionIcon>
            <ActionIcon
              size="lg"
              disabled={users && users.length < ITEM_COUNT_PER_PAGE}
              onClick={() => setPage(page + 1)}
            >
              <Icon path={mdiArrowRightBold} size={1} />
            </ActionIcon>
          </Group>
        </>
      }
    >
      <Paper shadow="md" p="xs" style={{ width: '100%' }}>
        <ScrollArea offsetScrollbars scrollbarSize={4} style={{ height: 'calc(100vh - 190px)' }}>
          <Table className={classes.table}>
            <thead>
              <tr>
                <th>Activated</th>
                <th>User</th>
                <th>Email</th>
                <th>User IP</th>
                <th>Real Name</th>
                <th>Student ID</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {users &&
                users.map((user) => (
                  <tr key={user.id}>
                    <td>
                      <Switch
                        disabled={disabled}
                        checked={user.emailConfirmed ?? false}
                        onChange={() => onToggleActive(user)}
                      />
                    </td>
                    <td>
                      <Group noWrap position="apart" spacing="xs">
                        <Group noWrap position="left">
                          <Avatar src={user.avatar} radius="xl" />
                          <Text weight={500} lineClamp={1}>
                            {user.userName}
                          </Text>
                        </Group>
                        <Badge size="sm" color={RoleColorMap.get(user.role ?? Role.User)}>
                          {user.role}
                        </Badge>
                      </Group>
                    </td>
                    <td>
                      <Text
                        size="sm"
                        style={{ fontFamily: theme.fontFamilyMonospace }}
                        lineClamp={1}
                      >
                        {user.email}
                      </Text>
                    </td>
                    <td>
                      <Group noWrap position="apart">
                        <Text
                          lineClamp={1}
                          size="sm"
                          style={{ fontFamily: theme.fontFamilyMonospace }}
                        >
                          {user.ip}
                        </Text>
                      </Group>
                    </td>
                    <td>{!user.realName ? 'Not set' : user.realName}</td>
                    <td>
                      <Text size="sm" style={{ fontFamily: theme.fontFamilyMonospace }}>
                        {!user.stdNumber ? '00000000' : user.stdNumber}
                      </Text>
                    </td>
                    <td align="right">
                      <Group noWrap spacing="sm" position="right">
                        <ActionIcon
                          color="blue"
                          onClick={() => {
                            setActiveUser(user)
                            setIsEditModalOpen(true)
                          }}
                        >
                          <Icon path={mdiPencilOutline} size={1} />
                        </ActionIcon>
                        <ActionIconWithConfirm
                          iconPath={mdiLockReset}
                          color="orange"
                          message={`Are you sure you want to reset the password for "${user.userName}"?`}
                          disabled={disabled}
                          onClick={() => onResetPassword(user)}
                        />
                        <ActionIconWithConfirm
                          iconPath={mdiDeleteOutline}
                          color="alert"
                          message={`Are you sure you want to delete "${user.userName}"?`}
                          disabled={disabled || user.id === currentUser?.userId}
                          onClick={() => onDelete(user)}
                        />
                      </Group>
                    </td>
                  </tr>
                ))}
            </tbody>
          </Table>
        </ScrollArea>
        <UserEditModal
          centered
          size="35%"
          title="Edit User"
          user={activeUser}
          opened={isEditModalOpen}
          onClose={() => setIsEditModalOpen(false)}
          mutateUser={(user: UserInfoModel) => {
            updateUsers(
              [user, ...(users?.filter((n) => n.id !== user.id) ?? [])].sort((a, b) =>
                a.id! < b.id! ? -1 : 1
              )
            )
          }}
        />
      </Paper>
    </AdminPage>
  )
}

export default Users
