import { FC, useEffect, useState } from 'react'
import { Button, Divider, SimpleGrid, Stack, Switch, TextInput, Title } from '@mantine/core'
import { mdiCheck, mdiContentSaveOutline } from '@mdi/js'
import { Icon } from '@mdi/react'
import AdminPage from '@Components/admin/AdminPage'
import { SwitchLabel } from '@Components/admin/SwitchLabel'
import { showErrorNotification } from '@Utils/ApiErrorHandler'
import { useFixedButtonStyles } from '@Utils/ThemeOverride'
import api, { AccountPolicy, ConfigEditModel, GlobalConfig } from '@Api'

const Configs: FC = () => {
  const { data: configs, mutate } = api.admin.useAdminGetConfigs({
    refreshInterval: 0,
    revalidateIfStale: false,
    revalidateOnFocus: false,
  })

  const [disabled, setDisabled] = useState(false)
  const [globalConfig, setGlobalConfig] = useState<GlobalConfig | null>()
  const [accountPolicy, setAccountPolicy] = useState<AccountPolicy | null>()
  const [saved, setSaved] = useState(true)
  const { classes: btnClasses } = useFixedButtonStyles({
    right: 'calc(0.05 * (100vw - 70px - 2rem) + 1rem)',
    bottom: '2rem',
  })

  useEffect(() => {
    if (configs) {
      setGlobalConfig(configs.globalConfig)
      setAccountPolicy(configs.accountPolicy)
    }
  }, [configs])

  const updateConfig = (conf: ConfigEditModel) => {
    setDisabled(true)
    api.admin
      .adminUpdateConfigs(conf)
      .then(() => {
        mutate({ ...conf })
      })
      .catch(showErrorNotification)
      .finally(() => {
        api.info.mutateInfoGetGlobalConfig()
        setDisabled(false)
      })
  }

  return (
    <AdminPage isLoading={!configs}>
      <Button
        className={btnClasses.fixedButton}
        variant="filled"
        radius="xl"
        size="md"
        leftIcon={<Icon path={saved ? mdiContentSaveOutline : mdiCheck} size={1} />}
        onClick={() => {
          updateConfig({ globalConfig, accountPolicy })
          setSaved(false)
          setTimeout(() => setSaved(true), 500)
        }}
        disabled={!saved}
      >
        Save Config
      </Button>
      <Stack style={{ width: '100%' }} spacing="xl">
        <Stack>
          <Title order={2}>Platform Settings</Title>
          <Divider />
          <SimpleGrid cols={2}>
            <TextInput
              label="Platform Name"
              description="Platform name will be shown in page title, top of page, etc. with ::CTF appended"
              placeholder="GZ"
              value={globalConfig?.title ?? ''}
              onChange={(e) => {
                setGlobalConfig({ ...(globalConfig ?? {}), title: e.currentTarget.value })
              }}
            />
            <TextInput
              label="Platform Slogan"
              description="Platform slogan will be shown at the top of page and about page"
              placeholder="Hack for fun not for profit"
              value={globalConfig?.slogan ?? ''}
              onChange={(e) => {
                setGlobalConfig({ ...(globalConfig ?? {}), slogan: e.currentTarget.value })
              }}
            />
          </SimpleGrid>
        </Stack>

        <Stack>
          <Title order={2}>Account Policy</Title>
          <Divider />
          <SimpleGrid cols={2}>
            <Switch
              checked={accountPolicy?.allowRegister ?? true}
              disabled={disabled}
              label={SwitchLabel('Enable user registration', 'Allow new users to register')}
              onChange={(e) =>
                setAccountPolicy({
                  ...(accountPolicy ?? {}),
                  allowRegister: e.currentTarget.checked,
                })
              }
            />
            <Switch
              checked={accountPolicy?.emailConfirmationRequired ?? false}
              disabled={disabled}
              label={SwitchLabel('Require email confirmation', 'Require email confirmation for user registration, email change and password reset')}
              onChange={(e) =>
                setAccountPolicy({
                  ...(accountPolicy ?? {}),
                  emailConfirmationRequired: e.currentTarget.checked,
                })
              }
            />
            <Switch
              checked={accountPolicy?.activeOnRegister ?? true}
              disabled={disabled}
              label={SwitchLabel('Active user on register', 'Automatically activate users on registration')}
              onChange={(e) =>
                setAccountPolicy({
                  ...(accountPolicy ?? {}),
                  activeOnRegister: e.currentTarget.checked,
                })
              }
            />
            <Switch
              checked={accountPolicy?.useGoogleRecaptcha ?? false}
              disabled={disabled}
              label={SwitchLabel('Use Google reCAPTCHA', 'Require Google reCAPTCHA for user registration, email change and password reset')}
              onChange={(e) =>
                setAccountPolicy({
                  ...(accountPolicy ?? {}),
                  useGoogleRecaptcha: e.currentTarget.checked,
                })
              }
            />
          </SimpleGrid>
          <TextInput
            label="Allowed Email Domains"
            description="Comma separated list of allowed email domains, leave empty to allow all domains"
            placeholder="No domain restriction"
            value={accountPolicy?.emailDomainList ?? ''}
            onChange={(e) => {
              setAccountPolicy({ ...(accountPolicy ?? {}), emailDomainList: e.currentTarget.value })
            }}
          />
        </Stack>
      </Stack>
    </AdminPage>
  )
}

export default Configs
