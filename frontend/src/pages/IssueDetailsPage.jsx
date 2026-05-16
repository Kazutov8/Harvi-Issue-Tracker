import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { issuesApi } from '../issues/issuesApi'

const PRIORITY_OPTIONS = ['low', 'medium', 'high', 'critical']
const STATUS_OPTIONS = ['backlog', 'todo', 'inProgress', 'inReview', 'done']

function normalizeStatusForSelect(status) {
  switch (status) {
    case 'in-progress':
    case 'inprogress':
      return 'inProgress'
    case 'in-review':
    case 'inreview':
      return 'inReview'
    default:
      return status
  }
}

function formatStatusLabel(status) {
  switch (status) {
    case 'inProgress':
      return 'in-progress'
    case 'inReview':
      return 'in-review'
    default:
      return status
  }
}

function IssueDetailsPage() {
  const { issueId } = useParams()
  const navigate = useNavigate()
  const { accessToken } = useAuth()
  const [issue, setIssue] = useState(null)
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [suggestion, setSuggestion] = useState(null)
  const [suggestionError, setSuggestionError] = useState('')
  const [isSuggesting, setIsSuggesting] = useState(false)
  const [availableUsers, setAvailableUsers] = useState([])
  const [triageForm, setTriageForm] = useState({ priority: 'medium', labelIds: [], acceptanceCriteria: '' })
  const [assigneeUserId, setAssigneeUserId] = useState('')
  const [selectedStatus, setSelectedStatus] = useState('backlog')
  const [actionError, setActionError] = useState('')
  const [actionSuccess, setActionSuccess] = useState('')
  const [isApplyingTriage, setIsApplyingTriage] = useState(false)
  const [isAssigning, setIsAssigning] = useState(false)
  const [isTransitioning, setIsTransitioning] = useState(false)

  async function loadIssueDetails(isActive) {
    setIsLoading(true)
    setError('')

    try {
      const [issueResponse, usersResponse] = await Promise.all([
        issuesApi.getById(issueId, accessToken),
        issuesApi.listUsers(accessToken),
      ])

      if (isActive) {
        setIssue(issueResponse)
        setAvailableUsers(usersResponse)
        setAssigneeUserId(issueResponse.assigneeUserId ?? '')
        setSelectedStatus(normalizeStatusForSelect(issueResponse.status))
      }
    } catch (loadError) {
      if (isActive) {
        setError(loadError.message)
      }
    } finally {
      if (isActive) {
        setIsLoading(false)
      }
    }
  }

  useEffect(() => {
    let isActive = true

    loadIssueDetails(isActive)

    return () => {
      isActive = false
    }
  }, [accessToken, issueId])

  async function handleSuggestTriage() {
    setIsSuggesting(true)
    setSuggestionError('')
    setActionError('')
    setActionSuccess('')
    setSuggestion(null)

    try {
      const suggestionResponse = await issuesApi.suggestTriage(issueId, accessToken)
      setSuggestion(suggestionResponse)
      if (suggestionResponse.isValid) {
        setTriageForm({
          priority: suggestionResponse.priority,
          labelIds: suggestionResponse.labels.map((label) => label.id),
          acceptanceCriteria: suggestionResponse.acceptanceCriteria ?? '',
        })
      }
    } catch (requestError) {
      let parsedError = null

      try {
        parsedError = JSON.parse(requestError.responseText ?? requestError.message)
      } catch {
        // Keep the raw message when it is not JSON.
      }

      if (requestError.status === 422 && parsedError?.issueId) {
        setSuggestion(parsedError)
        setSuggestionError('')
      } else {
        const message = parsedError?.message ?? requestError.message
        setSuggestion(null)
        setSuggestionError(message)
      }
    } finally {
      setIsSuggesting(false)
    }
  }

  async function handleApplyTriage(event) {
    event.preventDefault()
    setIsApplyingTriage(true)
    setActionError('')
    setActionSuccess('')

    try {
      const updatedIssue = await issuesApi.applyTriage(
        issueId,
        {
          priority: triageForm.priority,
          labelIds: triageForm.labelIds,
          acceptanceCriteria: triageForm.acceptanceCriteria,
        },
        accessToken,
      )

      setIssue(updatedIssue)
      setActionSuccess('AI triage suggestion was applied.')
    } catch (requestError) {
      setActionError(requestError.message)
    } finally {
      setIsApplyingTriage(false)
    }
  }

  async function handleAssign(event) {
    event.preventDefault()
    setIsAssigning(true)
    setActionError('')
    setActionSuccess('')

    try {
      const updatedIssue = await issuesApi.assign(issueId, assigneeUserId, accessToken)
      setIssue(updatedIssue)
      setActionSuccess('Issue assignee was updated.')
    } catch (requestError) {
      setActionError(requestError.message)
    } finally {
      setIsAssigning(false)
    }
  }

  async function handleTransition(event) {
    event.preventDefault()
    setIsTransitioning(true)
    setActionError('')
    setActionSuccess('')

    try {
      const updatedIssue = await issuesApi.transition(issueId, selectedStatus, accessToken)
      setIssue(updatedIssue)
      setSelectedStatus(normalizeStatusForSelect(updatedIssue.status))
      setActionSuccess('Issue status was updated.')
    } catch (requestError) {
      setActionError(requestError.message)
    } finally {
      setIsTransitioning(false)
    }
  }

  function handleLabelToggle(labelId) {
    setTriageForm((current) => ({
      ...current,
      labelIds: current.labelIds.includes(labelId)
        ? current.labelIds.filter((currentLabelId) => currentLabelId !== labelId)
        : [...current.labelIds, labelId],
    }))
  }

  if (error) {
    return (
      <main className="page-shell projects-shell">
        <section className="projects-layout">
          <p className="form-error">{error}</p>
          <button type="button" className="secondary-button" onClick={() => navigate('/projects')}>
            Back to projects
          </button>
        </section>
      </main>
    )
  }

  return (
    <main className="page-shell projects-shell">
      <section className="projects-layout">
        {isLoading || !issue ? (
          <p className="auth-subtitle">Loading issue...</p>
        ) : (
          <section className="projects-panel issue-details-panel">
            <div className="issue-details-header">
              <div>
                <span className="eyebrow">Issue details</span>
                <h1>{issue.title}</h1>
              </div>
              <Link to="/projects" className="secondary-button nav-link-button">
                Back to projects
              </Link>
            </div>

            <div className="issue-meta-grid">
              <div>
                <span className="issue-meta-label">Status</span>
                <strong>{issue.status}</strong>
              </div>
              <div>
                <span className="issue-meta-label">Priority</span>
                <strong>{issue.priority}</strong>
              </div>
              <div>
                <span className="issue-meta-label">Reporter</span>
                <strong>{issue.reporterUserId}</strong>
              </div>
              <div>
                <span className="issue-meta-label">Assignee</span>
                <strong>{issue.assigneeUserId ?? 'Unassigned'}</strong>
              </div>
            </div>

            <section className="issue-section">
              <h2>Description</h2>
              <p className="auth-subtitle">{issue.description || 'No description provided.'}</p>
            </section>

            <section className="issue-section">
              <h2>Labels</h2>
              {issue.labels.length === 0 ? (
                <p className="auth-subtitle">No labels assigned yet.</p>
              ) : (
                <ul className="label-list">
                  {issue.labels.map((label) => (
                    <li key={label.id} className="label-chip">
                      {label.name}
                    </li>
                  ))}
                </ul>
              )}
            </section>

            <section className="issue-section">
              <h2>Acceptance criteria</h2>
              <p className="auth-subtitle">{issue.acceptanceCriteria || 'No acceptance criteria yet.'}</p>
              {issue.acceptanceCriteriaIsAiGenerated ? <p className="issue-meta-label">Marked as AI-suggested</p> : null}
            </section>

            <section className="issue-section triage-panel">
              <div className="triage-panel-header">
                <div>
                  <h2>AI triage</h2>
                  <p className="auth-subtitle">Get a suggested priority, labels, and acceptance criteria draft.</p>
                </div>
                <button type="button" className="primary-button" onClick={handleSuggestTriage} disabled={isSuggesting}>
                  {isSuggesting ? 'Suggesting...' : 'AI suggest'}
                </button>
              </div>

              {suggestionError ? <p className="form-error">{suggestionError}</p> : null}

              {suggestion ? (
                suggestion.isValid ? (
                  <div className="triage-suggestion-card">
                    <form className="triage-edit-form" onSubmit={handleApplyTriage}>
                      <label>
                        <span>Priority</span>
                        <select
                          value={triageForm.priority}
                          onChange={(event) => setTriageForm((current) => ({ ...current, priority: event.target.value }))}
                        >
                          {PRIORITY_OPTIONS.map((priority) => (
                            <option key={priority} value={priority}>
                              {priority}
                            </option>
                          ))}
                        </select>
                      </label>

                      <div>
                        <span className="issue-meta-label">Suggested labels</span>
                        {suggestion.labels.length === 0 ? (
                          <p className="auth-subtitle">No labels suggested.</p>
                        ) : (
                          <div className="checkbox-list">
                            {suggestion.labels.map((label) => (
                              <label key={label.id} className="checkbox-item">
                                <input
                                  type="checkbox"
                                  checked={triageForm.labelIds.includes(label.id)}
                                  onChange={() => handleLabelToggle(label.id)}
                                />
                                <span>{label.name}</span>
                              </label>
                            ))}
                          </div>
                        )}
                      </div>

                      <label>
                        <span>Acceptance criteria draft</span>
                        <textarea
                          rows={5}
                          value={triageForm.acceptanceCriteria}
                          onChange={(event) =>
                            setTriageForm((current) => ({ ...current, acceptanceCriteria: event.target.value }))
                          }
                        />
                      </label>

                      <button type="submit" className="primary-button" disabled={isApplyingTriage}>
                        {isApplyingTriage ? 'Applying...' : 'Apply suggestion'}
                      </button>
                    </form>
                  </div>
                ) : (
                  <div className="triage-invalid-state">
                    <p className="form-error">{suggestion.validationError || 'The AI suggestion was invalid.'}</p>
                  </div>
                )
              ) : null}
            </section>

            <section className="issue-section triage-panel">
              <h2>Manual preparation</h2>

              <div className="triage-actions-grid">
                <form className="triage-suggestion-card" onSubmit={handleAssign}>
                  <label>
                    <span>Assignee</span>
                    <select value={assigneeUserId} onChange={(event) => setAssigneeUserId(event.target.value)}>
                      <option value="">Select a user</option>
                      {availableUsers.map((user) => (
                        <option key={user.id} value={user.id}>
                          {user.displayName} ({user.email})
                        </option>
                      ))}
                    </select>
                  </label>

                  <button type="submit" className="primary-button" disabled={isAssigning || !assigneeUserId}>
                    {isAssigning ? 'Assigning...' : 'Assign issue'}
                  </button>
                </form>

                <form className="triage-suggestion-card" onSubmit={handleTransition}>
                  <label>
                    <span>Status</span>
                    <select value={selectedStatus} onChange={(event) => setSelectedStatus(event.target.value)}>
                      {STATUS_OPTIONS.map((status) => (
                        <option key={status} value={status}>
                          {formatStatusLabel(status)}
                        </option>
                      ))}
                    </select>
                  </label>

                  <button type="submit" className="primary-button" disabled={isTransitioning}>
                    {isTransitioning ? 'Updating...' : 'Update status'}
                  </button>
                </form>
              </div>

              {actionError ? <p className="form-error">{actionError}</p> : null}
              {actionSuccess ? <p className="form-success">{actionSuccess}</p> : null}
            </section>
          </section>
        )}
      </section>
    </main>
  )
}

export default IssueDetailsPage
