import type { UseQueryResult } from "@tanstack/react-query";
import type { TestProfileDto, TestRunDto } from "../features/testing/types";

type TestingPageProps = {
  testProfilesQuery: UseQueryResult<TestProfileDto[], Error>;
  selectedProfile: TestProfileDto | undefined;
  selectedProfileId: string | null;
  setSelectedProfileId: (id: string) => void;
  newProfileName: string;
  setNewProfileName: (value: string) => void;
  newProfileDescription: string;
  setNewProfileDescription: (value: string) => void;
  handleCreateProfile: () => void;
  creatingProfile: boolean;
  stepType: string;
  setStepType: (value: string) => void;
  stepName: string;
  setStepName: (value: string) => void;
  stepSlaveAddress: string;
  setStepSlaveAddress: (value: string) => void;
  stepRegisterAddress: string;
  setStepRegisterAddress: (value: string) => void;
  stepValue: string;
  setStepValue: (value: string) => void;
  stepMinValue: string;
  setStepMinValue: (value: string) => void;
  stepMaxValue: string;
  setStepMaxValue: (value: string) => void;
  stepDelayMs: string;
  setStepDelayMs: (value: string) => void;
  handleAddStep: () => void;
  addingStep: boolean;
  runProfile: (id: string) => void;
  runPending: boolean;
  lastRun: TestRunDto | null;
  testRunsQuery: UseQueryResult<TestRunDto[], Error>;
  getStatusClass: (status: number | string) => string;
  formatTimestamp: (value: string) => string;
  canManageProfiles: boolean;
  canRunTests: boolean;
  unavailableReason: string;
};

export function TestingPage(props: TestingPageProps) {
  const {
    testProfilesQuery,
    selectedProfile,
    selectedProfileId,
    setSelectedProfileId,
    newProfileName,
    setNewProfileName,
    newProfileDescription,
    setNewProfileDescription,
    handleCreateProfile,
    creatingProfile,
    stepType,
    setStepType,
    stepName,
    setStepName,
    stepSlaveAddress,
    setStepSlaveAddress,
    stepRegisterAddress,
    setStepRegisterAddress,
    stepValue,
    setStepValue,
    stepMinValue,
    setStepMinValue,
    stepMaxValue,
    setStepMaxValue,
    stepDelayMs,
    setStepDelayMs,
    handleAddStep,
    addingStep,
    runProfile,
    runPending,
    lastRun,
    testRunsQuery,
    getStatusClass,
    formatTimestamp,
    canManageProfiles,
    canRunTests,
    unavailableReason,
  } = props;

  return (
    <section className="layout">
      <aside className="panel">
        <div className="panel-header">
          <h2>Тестовые профили</h2>
        </div>

        <div className="create-card">
          {!canManageProfiles && <p className="muted">{unavailableReason}</p>}
          <label>
            Название
            <input
              value={newProfileName}
              onChange={(event) => setNewProfileName(event.target.value)}
              disabled={!canManageProfiles}
            />
          </label>
          <label>
            Описание
            <textarea
              value={newProfileDescription}
              onChange={(event) => setNewProfileDescription(event.target.value)}
              disabled={!canManageProfiles}
            />
          </label>
          <button
            className="primary-button full-width"
            onClick={handleCreateProfile}
            disabled={creatingProfile || !canManageProfiles}
          >
            {creatingProfile ? "Создание..." : "Создать профиль"}
          </button>
        </div>

        <div className="device-list">
          {testProfilesQuery.data?.map((profile) => (
            <button
              key={profile.id}
              className={selectedProfileId === profile.id ? "device-card selected" : "device-card"}
              onClick={() => setSelectedProfileId(profile.id)}
            >
              <span className="device-title">{profile.name}</span>
              <span className="device-meta">Шагов: {profile.steps.length}</span>
            </button>
          ))}
        </div>
      </aside>

      <section className="panel content-panel">
        {!selectedProfile && <div className="empty-state">Выберите профиль или создайте новый.</div>}

        {selectedProfile && (
          <>
            <div className="panel-header">
              <h2>{selectedProfile.name}</h2>
              <div>
                {!canRunTests && <p className="muted">{unavailableReason}</p>}
                <button
                  className="primary-button"
                  onClick={() => runProfile(selectedProfile.id)}
                  disabled={runPending || !canRunTests}
                >
                  {runPending ? "Выполнение..." : "Запустить тест"}
                </button>
              </div>
            </div>

            <section className="operation-panel">
              <h3>Добавить шаг</h3>
              {!canManageProfiles && <p className="muted">{unavailableReason}</p>}
              <div className="step-form">
                <select
                  value={stepType}
                  onChange={(event) => setStepType(event.target.value)}
                  disabled={!canManageProfiles}
                >
                  <option value="WriteRegister">Записать регистр</option>
                  <option value="Delay">Пауза</option>
                  <option value="CheckRegisterRange">Проверить диапазон регистра</option>
                </select>
                <input
                  value={stepName}
                  onChange={(event) => setStepName(event.target.value)}
                  placeholder="Название шага"
                  disabled={!canManageProfiles}
                />
                {stepType !== "Delay" && (
                  <>
                    <input
                      value={stepSlaveAddress}
                      onChange={(event) => setStepSlaveAddress(event.target.value)}
                      placeholder="Slave"
                      disabled={!canManageProfiles}
                    />
                    <input
                      value={stepRegisterAddress}
                      onChange={(event) => setStepRegisterAddress(event.target.value)}
                      placeholder="Регистр"
                      disabled={!canManageProfiles}
                    />
                  </>
                )}
                {stepType === "WriteRegister" && (
                  <input
                    value={stepValue}
                    onChange={(event) => setStepValue(event.target.value)}
                    placeholder="Значение"
                    disabled={!canManageProfiles}
                  />
                )}
                {stepType === "CheckRegisterRange" && (
                  <>
                    <input
                      value={stepMinValue}
                      onChange={(event) => setStepMinValue(event.target.value)}
                      placeholder="Min"
                      disabled={!canManageProfiles}
                    />
                    <input
                      value={stepMaxValue}
                      onChange={(event) => setStepMaxValue(event.target.value)}
                      placeholder="Max"
                      disabled={!canManageProfiles}
                    />
                  </>
                )}
                {stepType === "Delay" && (
                  <input
                    value={stepDelayMs}
                    onChange={(event) => setStepDelayMs(event.target.value)}
                    placeholder="Задержка, мс"
                    disabled={!canManageProfiles}
                  />
                )}
                <button
                  className="secondary-button"
                  onClick={handleAddStep}
                  disabled={addingStep || !canManageProfiles}
                >
                  {addingStep ? "Добавление..." : "Добавить шаг"}
                </button>
              </div>
            </section>

            <div className="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Шаг</th>
                    <th>Тип</th>
                  </tr>
                </thead>
                <tbody>
                  {selectedProfile.steps.map((step) => (
                    <tr key={step.id}>
                      <td>{step.orderIndex}</td>
                      <td>{step.name}</td>
                      <td>{step.type}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {lastRun && (
              <div className="table-wrapper">
                <table>
                  <thead>
                    <tr>
                      <th>Шаг</th>
                      <th>Статус</th>
                      <th>Сообщение</th>
                    </tr>
                  </thead>
                  <tbody>
                    {lastRun.steps.map((step) => (
                      <tr key={step.id}>
                        <td>{step.stepName}</td>
                        <td>
                          <span className={getStatusClass(step.status)}>{step.status}</span>
                        </td>
                        <td>{step.message}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}

            <div className="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Время</th>
                    <th>Профиль</th>
                    <th>Статус</th>
                  </tr>
                </thead>
                <tbody>
                  {testRunsQuery.data?.map((run) => (
                    <tr key={run.id}>
                      <td>{formatTimestamp(run.startedAtUtc)}</td>
                      <td>{run.profileName}</td>
                      <td>
                        <span className={getStatusClass(run.status)}>{run.status}</span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </>
        )}
      </section>
    </section>
  );
}
