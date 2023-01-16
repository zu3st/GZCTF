import dayjs from 'dayjs'
import { FC, useEffect, useState } from 'react'
import {
  Avatar,
  Text,
  Button,
  Center,
  Grid,
  Group,
  Input,
  Modal,
  ModalProps,
  SegmentedControl,
  SimpleGrid,
  Stack,
  Textarea,
  TextInput,
} from '@mantine/core'
import { showNotification } from '@mantine/notifications'
import { mdiCheck } from '@mdi/js'
import { Icon } from '@mdi/react'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import api, { UserInfoModel, UpdateUserInfoModel, Role } from '@Api'

export const RoleColorMap = new Map<Role, string>([
  [Role.Admin, 'blue'],
  [Role.User, 'brand'],
  [Role.Monitor, 'yellow'],
  [Role.Banned, 'red'],
])

interface UserEditModalProps extends ModalProps {
  user: UserInfoModel
  mutateUser: (user: UserInfoModel) => void
}

const UserEditModal: FC<UserEditModalProps> = (props) => {
  const { user, mutateUser, ...modalProps } = props

  const [disabled, setDisabled] = useState(false)

  const [activeUser, setActiveUser] = useState<UserInfoModel>(user)
  const [profile, setProfile] = useState<UpdateUserInfoModel>({})

  useEffect(() => {
    setProfile({
      userName: user.userName,
      email: user.email,
      role: user.role,
      bio: user.bio,
      realName: user.realName,
      stdNumber: user.stdNumber,
      phone: user.phone,
    })
    setActiveUser(user)
  }, [user])

  const onChangeProfile = () => {
    setDisabled(true)
    api.admin
      .adminUpdateUserInfo(activeUser.id!, profile)
      .then(() => {
        showNotification({
          color: 'teal',
          message: 'User information updated',
          icon: <Icon path={mdiCheck} size={1} />,
          disallowClose: true,
        })
        mutateUser({ ...activeUser, ...profile })
        modalProps.onClose()
      })
      .catch(showErrorNotification)
      .finally(() => {
        setDisabled(false)
      })
  }

  return (
    <Modal {...modalProps}>
      {/* User Info */}
      <Stack spacing="md" style={{ margin: 'auto', marginTop: '15px' }}>
        <Grid grow>
          <Grid.Col span={8}>
            <TextInput
              label="Username"
              type="text"
              style={{ width: '100%' }}
              value={profile.userName ?? 'ctfer'}
              disabled={disabled}
              onChange={(event) => setProfile({ ...profile, userName: event.target.value })}
            />
          </Grid.Col>
          <Grid.Col span={4}>
            <Center>
              <Avatar radius="xl" size={70} src={activeUser.avatar} />
            </Center>
          </Grid.Col>
        </Grid>
        <Input.Wrapper label="Role">
          <SegmentedControl
            fullWidth
            disabled={disabled}
            color={RoleColorMap.get(profile.role ?? Role.User)}
            value={profile.role ?? Role.User}
            onChange={(value: Role) => setProfile({ ...profile, role: value })}
            data={Object.entries(Role).map((role) => ({
              value: role[1],
              label: role[0],
            }))}
          />
        </Input.Wrapper>
        <SimpleGrid cols={2}>
          <TextInput
            label="Email"
            type="email"
            style={{ width: '100%' }}
            value={profile.email ?? 'ctfer@gzti.me'}
            disabled={disabled}
            onChange={(event) => setProfile({ ...profile, email: event.target.value })}
          />
          <TextInput
            label="Phone"
            type="tel"
            style={{ width: '100%' }}
            value={profile.phone ?? ''}
            disabled={disabled}
            onChange={(event) => setProfile({ ...profile, phone: event.target.value })}
          />
          <TextInput
            label="Matriculation Number"
            type="number"
            style={{ width: '100%' }}
            value={profile.stdNumber ?? ''}
            disabled={disabled}
            onChange={(event) => setProfile({ ...profile, stdNumber: event.target.value })}
          />
          <TextInput
            label="Real Name"
            type="text"
            style={{ width: '100%' }}
            value={profile.realName ?? ''}
            disabled={disabled}
            onChange={(event) => setProfile({ ...profile, realName: event.target.value })}
          />
        </SimpleGrid>
        <Textarea
          label="Bio"
          value={profile.bio ?? 'Apparently, this user prefers to keep an air of mystery about them'}
          style={{ width: '100%' }}
          disabled={disabled}
          autosize
          minRows={2}
          maxRows={4}
          onChange={(event) => setProfile({ ...profile, bio: event.target.value })}
        />

        <Stack spacing={2}>
          <Group position="apart">
            <Text size="sm" weight={500}>
              User IP
            </Text>
            <Text
              size="sm"
              span
              weight={500}
              sx={(theme) => ({ fontFamily: theme.fontFamilyMonospace })}
            >
              {user.ip}
            </Text>
          </Group>
          <Group position="apart">
            <Text size="sm" weight={500}>
              Last Visited
            </Text>
            <Text
              size="sm"
              span
              weight={500}
              sx={(theme) => ({ fontFamily: theme.fontFamilyMonospace })}
            >
              {dayjs(user.lastVisitedUTC).format('YYYY-MM-DD HH:mm:ss')}
            </Text>
          </Group>
        </Stack>

        <Group grow style={{ margin: 'auto', width: '100%' }}>
          <Button fullWidth disabled={disabled} onClick={onChangeProfile}>
            Save Changes
          </Button>
        </Group>
      </Stack>
    </Modal>
  )
}

export default UserEditModal
