import React, { FC } from 'react'
import { Text, Box, Center, PasswordInput, Popover, Progress } from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { mdiCheck, mdiClose } from '@mdi/js'
import { Icon } from '@mdi/react'
import { useIsMobile } from '@Utils/ThemeOverride'

const PasswordRequirement: FC<{ meets: boolean; label: string }> = ({ meets, label }) => {
  return (
    <Text color={meets ? 'teal' : 'red'} mt={5} size="sm">
      <Center inline>
        {meets ? <Icon path={mdiCheck} size={0.7} /> : <Icon path={mdiClose} size={0.7} />}
        <Box ml={7}>{label}</Box>
      </Center>
    </Text>
  )
}

const requirements = [
  { re: /[0-9]/, label: 'Contains numbers' },
  { re: /[a-z]/, label: 'Contains lowercase letters' },
  { re: /[A-Z]/, label: 'Contains uppercase letters' },
  { re: /[`$&+,:;=?@#|'<>.^*()%!-]/, label: 'Contains special characters' },
]

const getStrength = (password: string) => {
  let multiplier = password.length > 5 ? 0 : 1

  requirements.forEach((requirement) => {
    if (!requirement.re.test(password)) {
      multiplier += 1
    }
  })

  return Math.max(100 - (100 / (requirements.length + 1)) * multiplier, 0)
}

interface StrengthPasswordInputProps {
  value: string
  disabled?: boolean
  label?: string
  onChange: React.ChangeEventHandler<HTMLInputElement>
  onKeyDown?: React.KeyboardEventHandler<HTMLInputElement>
}

const StrengthPasswordInput: FC<StrengthPasswordInputProps> = (props) => {
  const [opened, { close, open }] = useDisclosure(false)
  const pwd = props.value
  const { isMobile } = useIsMobile()

  const checks = [
    <PasswordRequirement key={0} label="At least 6 characters" meets={pwd.length >= 6} />,
    ...requirements.map((requirement, index) => (
      <PasswordRequirement
        key={index + 1}
        label={requirement.label}
        meets={requirement.re.test(pwd)}
      />
    )),
  ]

  const strength = getStrength(pwd)
  const color = strength === 100 ? 'teal' : strength > 50 ? 'yellow' : 'red'

  return (
    <Popover
      opened={opened}
      position={isMobile ? 'top' : 'right'}
      styles={{
        dropdown: {
          marginLeft: '2rem',
          width: isMobile ? '50vw' : '10rem',
        },
      }}
      withArrow
      transition="pop-bottom-left"
    >
      <Popover.Target>
        <PasswordInput
          required
          label={props.label ?? 'Password'}
          placeholder="P4ssW@rd"
          value={props.value}
          onFocusCapture={open}
          onBlurCapture={close}
          disabled={props.disabled}
          onChange={props.onChange}
          style={{ width: '100%' }}
        />
      </Popover.Target>
      <Popover.Dropdown>
        <Progress color={color} value={strength} size={5} style={{ marginBottom: 10 }} />
        {checks}
      </Popover.Dropdown>
    </Popover>
  )
}

export default StrengthPasswordInput
