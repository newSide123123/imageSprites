{{- define "common.fullname" -}}
{{ .Release.Name }}-{{ .Chart.Name }}
{{- end -}}