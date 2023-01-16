import { FC, useEffect, useState } from 'react'
import {
  Box,
  Stack,
  Group,
  Divider,
  Text,
  Button,
  Grid,
  TextInput,
  Paper,
  Textarea,
  Modal,
  Avatar,
  Image,
  Center,
  SimpleGrid,
} from '@mantine/core'
import { Dropzone } from '@mantine/dropzone'
import { showNotification } from '@mantine/notifications'
import { mdiCheck, mdiClose } from '@mdi/js'
import { Icon } from '@mdi/react'
import PasswordChangeModal from '@Components/PasswordChangeModal'
import WithNavBar from '@Components/WithNavbar'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import { ACCEPT_IMAGE_MIME_TYPE, useIsMobile } from '@Utils/ThemeOverride'
import { usePageTitle } from '@Utils/usePageTitle'
import { useUser } from '@Utils/useUser'
import api, { ProfileUpdateModel } from '@Api'

const Profile: FC = () => {
  const [dropzoneOpened, setDropzoneOpened] = useState(false)
  const { user, mutate } = useUser()

  const [profile, setProfile] = useState<ProfileUpdateModel>({
    userName: user?.userName,
    bio: user?.bio,
    stdNumber: user?.stdNumber,
    phone: user?.phone,
    realName: user?.realName,
  })
  const [avatarFile, setAvatarFile] = useState<File | null>(null)

  const [disabled] = useState(false)

  const [mailEditOpened, setMailEditOpened] = useState(false)
  const [pwdChangeOpened, setPwdChangeOpened] = useState(false)

  const [email, setEmail] = useState('')

  const { isMobile } = useIsMobile()

  usePageTitle('Profile')

  useEffect(() => {
    setProfile({
      userName: user?.userName,
      bio: user?.bio,
      stdNumber: user?.stdNumber,
      phone: user?.phone,
      realName: user?.realName,
    })
  }, [user])

  const onChangeAvatar = () => {
    if (avatarFile) {
      api.account
        .accountAvatar({
          file: avatarFile,
        })
        .then(() => {
          showNotification({
            color: 'teal',
            message: 'Your avatar has been updated',
            icon: <Icon path={mdiCheck} size={1} />,
            disallowClose: true,
          })
          mutate({ ...user })
          setAvatarFile(null)
          setDropzoneOpened(false)
        })
        .catch((err) => {
          showErrorNotification(err)
          setDropzoneOpened(false)
        })
    }
  }

  const onChangeProfile = () => {
    api.account
      .accountUpdate(profile)
      .then(() => {
        showNotification({
          color: 'teal',
          title: 'Profile updated',
          message: 'Your profile has been updated',
          icon: <Icon path={mdiCheck} size={1} />,
          disallowClose: true,
        })
        mutate({ ...user })
      })
      .catch(showErrorNotification)
  }

  const onChangeEmail = () => {
    if (email) {
      api.account
        .accountChangeEmail({
          newMail: email,
        })
        .then((res) => {
          if (res.data.data) {
            showNotification({
              color: 'teal',
              title: 'Verification email has been sent',
              message: 'Please check your mailbox and spam box',
              icon: <Icon path={mdiCheck} size={1} />,
              disallowClose: true,
            })
          } else {
            mutate({ ...user, email: email })
          }
          setMailEditOpened(false)
        })
        .catch(showErrorNotification)
    }
  }

  const context = (
    <>
      {/* Header */}
      <Box style={{ marginBottom: '5px' }}>
        <h2>Personal Information</h2>
      </Box>
      <Divider />

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
              <Avatar
                radius={40}
                size={80}
                src={user?.avatar}
                onClick={() => setDropzoneOpened(true)}
              />
            </Center>
          </Grid.Col>
        </Grid>
        <TextInput
          label="Email"
          type="email"
          style={{ width: '100%' }}
          value={user?.email ?? 'ctfer@gzti.me'}
          disabled
          readOnly
        />
        <TextInput
          label="Phone Number"
          type="tel"
          style={{ width: '100%' }}
          value={profile.phone ?? ''}
          disabled={disabled}
          onChange={(event) => setProfile({ ...profile, phone: event.target.value })}
        />
        <SimpleGrid cols={2}>
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
        <Box style={{ margin: 'auto', width: '100%' }}>
          <Grid grow>
            <Grid.Col span={4}>
              <Button
                fullWidth
                color="orange"
                variant="outline"
                disabled={disabled}
                onClick={() => setMailEditOpened(true)}
              >
                Change Email
              </Button>
            </Grid.Col>
            <Grid.Col span={4}>
              <Button
                fullWidth
                color="orange"
                variant="outline"
                disabled={disabled}
                onClick={() => setPwdChangeOpened(true)}
              >
                Change Password
              </Button>
            </Grid.Col>
            <Grid.Col span={4}>
              <Button fullWidth disabled={disabled} onClick={onChangeProfile}>
              Save
              </Button>
            </Grid.Col>
          </Grid>
        </Box>
      </Stack>
    </>
  )

  return (
    <WithNavBar minWidth={0}>
      {isMobile ? (
        context
      ) : (
        <Center style={{ height: '90vh' }}>
          <Paper style={{ width: '55%', maxWidth: 600 }} shadow="sm" pt="2%" p="5%">
            {context}
          </Paper>
        </Center>
      )}

      {/* Change Password */}
      <PasswordChangeModal
        opened={pwdChangeOpened}
        centered
        onClose={() => setPwdChangeOpened(false)}
        title="Change Password"
      />

      {/* Change Email */}
      <Modal
        opened={mailEditOpened}
        centered
        onClose={() => setMailEditOpened(false)}
        title="Change Email"
      >
        <Stack>
          <Text>
            After changing your email, you will not be able to log in with the original
          </Text>
          <TextInput
            required
            label="New Email"
            type="email"
            style={{ width: '100%' }}
            placeholder={user?.email ?? 'ctfer@gzti.me'}
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
          <Group position="right">
            <Button
              variant="default"
              onClick={() => {
                setEmail(user?.email ?? '')
                setMailEditOpened(false)
              }}
            >
              Cancel
            </Button>
            <Button color="orange" onClick={onChangeEmail}>
              Confirm
            </Button>
          </Group>
        </Stack>
      </Modal>

      {/* Change avatar */}
      <Modal
        opened={dropzoneOpened}
        onClose={() => setDropzoneOpened(false)}
        centered
        withCloseButton={false}
      >
        <Dropzone
          onDrop={(files) => setAvatarFile(files[0])}
          onReject={() => {
            showNotification({
              color: 'red',
              title: 'Avatar Upload Failed',
              message: 'Please check the file format and size',
              icon: <Icon path={mdiClose} size={1} />,
              disallowClose: true,
            })
          }}
          style={{
            margin: '0 auto 20px auto',
            minWidth: '220px',
            minHeight: '220px',
          }}
          maxSize={3 * 1024 * 1024}
          accept={ACCEPT_IMAGE_MIME_TYPE}
        >
          <Group position="center" spacing="xl" style={{ minHeight: 240, pointerEvents: 'none' }}>
            {avatarFile ? (
              <Image fit="contain" src={URL.createObjectURL(avatarFile)} alt="avatar" />
            ) : (
              <Box>
                <Text size="xl" inline>
                  Drag and drop or click here to select an avatar
                </Text>
                <Text size="sm" color="dimmed" inline mt={7}>
                  Please upload an image file with a maximum size of 3MB
                </Text>
              </Box>
            )}
          </Group>
        </Dropzone>
        <Button fullWidth variant="outline" disabled={disabled} onClick={onChangeAvatar}>
          Update Avatar
        </Button>
      </Modal>
    </WithNavBar>
  )
}

export default Profile
