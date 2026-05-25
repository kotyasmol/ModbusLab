import type { UseQueryResult } from "@tanstack/react-query";
import type { TestProfileDto, TestRunDto } from "../features/testing/types";

type Props = {
  testProfilesQuery: UseQueryResult<TestProfileDto[], Error>;
  selectedProfile: TestProfileDto | undefined;
  selectedProfileId: string | null;
  setSelectedProfileId: (id: string) => void;
  newProfileName: string;
  setNewProfileName: (v: string) => void;
  newProfileDescription: string;
  setNewProfileDescription: (v: string) => void;
  handleCreateProfile: () => void;
  creatingProfile: boolean;
  stepType: string; setStepType: (v: string)=>void;
  stepName: string; setStepName: (v: string)=>void;
  stepSlaveAddress: string; setStepSlaveAddress: (v: string)=>void;
  stepRegisterAddress: string; setStepRegisterAddress: (v: string)=>void;
  stepValue: string; setStepValue: (v: string)=>void;
  stepMinValue: string; setStepMinValue: (v: string)=>void;
  stepMaxValue: string; setStepMaxValue: (v: string)=>void;
  stepDelayMs: string; setStepDelayMs: (v: string)=>void;
  handleAddStep: ()=>void; addingStep:boolean;
  runProfile: (id:string)=>void; runPending:boolean;
  lastRun: TestRunDto | null;
  testRunsQuery: UseQueryResult<TestRunDto[], Error>;
  getStatusClass: (status:number|string)=>string;
  formatTimestamp: (d:string)=>string;
};
export function TestingPage(p:Props){return <section className="layout"><aside className="panel"><div className="panel-header"><h2>Тестовые профили</h2></div><div className="create-card"><input value={p.newProfileName} onChange={(e)=>p.setNewProfileName(e.target.value)} /><textarea value={p.newProfileDescription} onChange={(e)=>p.setNewProfileDescription(e.target.value)} /><button className="primary-button full-width" onClick={p.handleCreateProfile} disabled={p.creatingProfile}>{p.creatingProfile?"Создание...":"Создать профиль"}</button></div><div className="device-list">{p.testProfilesQuery.data?.map((profile)=><button key={profile.id} className={p.selectedProfileId===profile.id?"device-card selected":"device-card"} onClick={()=>p.setSelectedProfileId(profile.id)}><span className="device-title">{profile.name}</span></button>)}</div></aside><section className="panel content-panel">{p.selectedProfile && <><button className="primary-button" onClick={()=>p.runProfile(p.selectedProfile!.id)} disabled={p.runPending}>{p.runPending?"Выполнение...":"Запустить тест"}</button><section className="operation-panel"><select value={p.stepType} onChange={(e)=>p.setStepType(e.target.value)}><option value="WriteRegister">Записать регистр</option><option value="Delay">Пауза</option><option value="CheckRegisterRange">Проверить диапазон регистра</option></select><input value={p.stepName} onChange={(e)=>p.setStepName(e.target.value)} /><input value={p.stepSlaveAddress} onChange={(e)=>p.setStepSlaveAddress(e.target.value)} /><input value={p.stepRegisterAddress} onChange={(e)=>p.setStepRegisterAddress(e.target.value)} /><button className="secondary-button" onClick={p.handleAddStep} disabled={p.addingStep}>Добавить шаг</button></section><div className="table-wrapper"><table><tbody>{p.selectedProfile.steps.map((s)=><tr key={s.id}><td>{s.orderIndex}</td><td>{s.name}</td><td>{s.type}</td></tr>)}</tbody></table></div>{p.lastRun && <div className="table-wrapper"><table><tbody>{p.lastRun.steps.map((s)=><tr key={s.id}><td>{s.stepName}</td><td><span className={p.getStatusClass(s.status)}>{s.status}</span></td></tr>)}</tbody></table></div>}<div className="table-wrapper"><table><tbody>{p.testRunsQuery.data?.map((r)=><tr key={r.id}><td>{p.formatTimestamp(r.startedAtUtc)}</td><td>{r.profileName}</td><td>{r.status}</td></tr>)}</tbody></table></div></>}</section></section>}
