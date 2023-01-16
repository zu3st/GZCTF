import React, { FC, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Button, Center, Group, Stack, Title, Text, ScrollArea } from '@mantine/core'
import { mdiFolderDownloadOutline, mdiKeyboardBackspace } from '@mdi/js'
import { Icon } from '@mdi/react'
import PDFViewer from '@Components/admin/PDFViewer'
import TeamWriteupCard from '@Components/admin/TeamWriteupCard'
import WithGameTab from '@Components/admin/WithGameEditTab'
import api, { WriteupInfoModel } from '@Api'

const GameWriteups: FC = () => {
  const { id } = useParams()
  const numId = parseInt(id ?? '-1')
  const navigate = useNavigate()
  const [selected, setSelected] = useState<WriteupInfoModel>()

  const { data: writeups } = api.admin.useAdminWriteups(numId, {
    refreshInterval: 0,
    revalidateIfStale: false,
  })

  useEffect(() => {
    if (writeups?.length && !selected) {
      setSelected(writeups[0])
    }
  }, [writeups])

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

          <Group grow miw="15rem" maw="15rem" position="apart">
            <Button
              fullWidth
              leftIcon={<Icon path={mdiFolderDownloadOutline} size={1} />}
              onClick={() => window.open(`/api/admin/writeups/${id}/all`)}
            >
              Download all writeups
            </Button>
          </Group>
        </>
      }
    >
      {!writeups?.length || !selected ? (
        <Center mih="calc(100vh - 180px)">
          <Stack spacing={0}>
            <Title order={2}>Ouch! No team has submitted writeup for this game</Title>
            <Text>New writeups will be shown here</Text>
          </Stack>
        </Center>
      ) : (
        <Group noWrap align="flex-start" position="apart">
          <Stack style={{ position: 'relative', marginTop: '-3rem', width: 'calc(100% - 16rem)' }}>
            <PDFViewer url={selected.url} height="calc(100vh - 110px)" />
          </Stack>
          <ScrollArea miw="15rem" maw="15rem" style={{ height: 'calc(100vh-180px)' }} type="auto">
            <Stack>
              {writeups?.map((writeup) => (
                <TeamWriteupCard
                  key={writeup.id}
                  writeup={writeup}
                  selected={selected?.id === writeup.id}
                  onClick={() => setSelected(writeup)}
                >
                  <Text>{writeup.team?.name}</Text>
                </TeamWriteupCard>
              ))}
            </Stack>
          </ScrollArea>
        </Group>
      )}
    </WithGameTab>
  )
}

export default GameWriteups
