export interface ErrorSource {
  type: string
  function: string
  fileName: string
  line: number
  preContextCode: string
  contextCode: string
  postContextCode: string
}

export interface MessageLogEntry {
  timeStamp: string
  level: number
  message: string
  exception?: string
  scope?: string
  params?: LogParam[]
  collapsed?: boolean
}

export interface LogParam {
  key: string
  value: unknown
  timeStamp?: string
}

export interface SqlLogEntry {
  timeStamp: string
  durationMs: number
  sqlText: string
}

export interface ErrorDetail {
  statusCode: number
  type: string
  message: string
  detail: string
  source: string
  time: string
  url: string
  method: string
  hostName: string
  applicationName: string
  client: string
  user: string
  os: string
  browser: string
  severity: string
  body: string
  htmlMessage: string
  sources: ErrorSource[]
  messageLog: MessageLogEntry[]
  sqlLog: SqlLogEntry[]
  queryString: Record<string, string>
  form: Record<string, string>
  header: Record<string, string>
  cookies: Record<string, string>
  connection: Record<string, string>
  serverVariables: Record<string, string>
  userData: Record<string, unknown>
  session: Record<string, unknown>
}

export interface ErrorListItem {
  id: string
  error: ErrorDetail
  log?: string
}

export interface ErrorsResponse {
  errors: ErrorListItem[]
  totalCount: number
}

export interface HelpHtmlResponse {
  html: string
  path: string
}

export interface CountryInfo {
  countryCode: string
  country: string
}

export interface Toast {
  id: number
  message: string
  variant: string
}
