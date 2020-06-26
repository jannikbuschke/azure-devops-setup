import React from "react"
import { Formik } from "formik"
import { Input, SubmitButton, Form } from "formik-antd"
import styled from "styled-components"
import { useSubmit } from"glow-react"
import { notification } from "antd"

function App() {
  const [submit, validate] = useSubmit("/api/run-setup")
  const [result,setResult]=React.useState<any>(null)
  return (
    <Container>
        <Formik
          initialValues={{ param1: "test", param2:"test..." }}
          onSubmit={async (values, f) => {
            const result = await submit({parameters:values})
            notification.info({message:JSON.stringify(result)})
            setResult(result)
            f.setSubmitting(false)
          }}
          >
          <Form>
            <Input name="param1" />
            <Input name="param2" />
            <SubmitButton
              style={{ marginTop: 10 }}
              >
              Submit
            </SubmitButton>
          </Form>
        </Formik>
              <div><pre>{JSON.stringify(result,null,4)}</pre></div>
    </Container>
  )
}

const Container = styled.div`
  min-height:100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
`

export default App
