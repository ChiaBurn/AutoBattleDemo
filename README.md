# Auto Battle Demo

## 1. 專案概要

本專案是依照實作題需求製作的 Unity 2D 回合制雙陣營自動戰鬥 Demo。遊戲中左方與右方隊伍各包含 4 種職業角色：戰士、精靈、法師、牧師。雙方依隊伍排列順序進行回合制行動，角色會自動攻擊敵方仍存活的隨機單一目標，並治療我方仍存活且目前 HP 最低的角色，直到其中一方全滅為止。

本專案重點不在美術表現，而在於回合制戰鬥邏輯、事件紀錄、SQLite 儲存、Replay 回放、AI 勝率模擬，以及 Unity UI 狀態管理。

| 功能        | 說明                            |
| --------- | ----------------------------- |
| 回合制自動戰鬥   | 左右雙方依角色排列順序行動                 |
| 戰鬥演出      | 顯示當前行動者、被攻擊者、被治療者             |
| 快轉與跳過     | `到結束` / `播放到結束` 支援快轉演出與跳過     |
| AI 建議配置   | 針對固定右隊配置，模擬左隊 24 種排列並選出最高勝率配置 |
| SQLite 儲存 | 戰鬥結束時一次性寫入戰鬥資料                |
| Replay 回放 | 依照已儲存事件資料重播戰鬥，不重新計算           |
| 歷史紀錄列表    | 可選擇過往戰鬥紀錄進行回放                 |
| PC Build  | 可從 Unity 專案自行輸出 Windows 可執行檔  |

---

## 2. 開發環境

| 項目                | 版本 / 說明                      |
| ----------------- | ---------------------------- |
| Engine            | Unity 6000.3.9f1             |
| Project Type      | 2D                           |
| Language          | C#                           |
| Database          | SQLite                       |
| SQLite Package    | Microsoft.Data.Sqlite 10.0.9 |
| Package Installer | NuGetForUnity                |
| Target Platform   | Windows PC                   |

---

## 3. 執行方式

### 3.1 使用 Unity Editor 執行

主要場景位於：

```text
Assets/_Project/Scenes/BattleScene.unity
```

開啟該場景後按下 Play 即可測試。

### 3.2 自行 Build Windows 可執行檔

可在 Unity 中開啟：

```text
File > Build Profiles
```

確認 `BattleScene.unity` 已加入 Scene List，並選擇 Windows / x86_64 作為目標平台後執行 Build。

Build 完成後，請在輸出資料夾中執行 `AutoBattleDemo.exe`。  
請保留輸出資料夾內的 `_Data`、`UnityPlayer.dll` 與相關 DLL，避免單獨移動 `.exe` 造成執行失敗。

---

## 4. 主要狀態機

本專案以 `BattlePhase` 控制 UI 顯示與操作狀態。閱讀 UI 流程時，建議先理解此狀態機。

| 狀態                     | 說明              |
| ---------------------- | --------------- |
| `NotStarted`           | 尚未開始            |
| `BattleReady`          | 戰鬥已建立，尚未進行回合    |
| `BattleInProgress`     | 戰鬥進行中           |
| `BattleResolvingRound` | 正在播放下一回合        |
| `BattleAutoResolving`  | 正在快轉到結束，可跳過     |
| `Saving`               | 正在儲存            |
| `FinishedSaved`        | 戰鬥結束且已儲存        |
| `LoadList`             | 顯示回放紀錄列表        |
| `ReplayReady`          | Replay 已載入，等待播放 |
| `ReplayPlaying`        | 正在播放下一回合 Replay |
| `ReplayAutoPlaying`    | 正在快轉 Replay，可跳過 |
| `ReplayFinished`       | Replay 播放結束     |
| `AiRunning`            | AI 計算中          |
| `AiResult`             | AI 結果等待套用或取消    |

---

## 5. 操作說明

| 狀態               | 按鈕         | 行為                      |
| ---------------- | ---------- | ----------------------- |
| 初始狀態             | `開始`       | 建立左右雙方隊伍                |
| 初始狀態 / 結束狀態      | `回放紀錄`     | 開啟已儲存戰鬥紀錄列表             |
| 戰鬥準備中            | `AI建議左隊配置` | 針對目前右隊配置，計算建議左隊排列       |
| 戰鬥準備中 / 進行中      | `下一回合`     | 播放下一回合，每個行動約 0.5 秒      |
| 戰鬥準備中 / 進行中      | `到結束`      | 快轉播放至戰鬥結束，每個行動約 0.1 秒   |
| 快轉中              | `跳過`       | 立即略過剩餘演出並顯示最終結果         |
| 戰鬥結束後            | `重播本場`     | 載入本場已儲存資料並進入 Replay     |
| Replay 準備中       | `播放下一回合`   | 逐步播放下一回合                |
| Replay 準備中 / 播放中 | `播放到結束`    | 快轉播放至 Replay 結束         |
| Replay 快轉中       | `跳過`       | 立即略過剩餘 Replay 演出        |
| 戰鬥中 / Replay 中   | `新的一輪`     | 建立新戰鬥；若舊戰鬥尚未結束，會背景結算並儲存 |

---

## 6. 視覺回饋規則

| 狀態         | 視覺表現        |
| ---------- | ----------- |
| 當前行動者      | 黃色外框        |
| 被攻擊者       | 紅色背景        |
| 被治療者       | 綠色背景        |
| 行動者同時是被治療者 | 黃色外框 + 綠色背景 |
| 倒下角色       | 灰色背景        |

---

## 7. 專案架構

本專案採用分層結構，將戰鬥規則、應用服務、資料存取與 Unity UI 表現層分開，避免核心邏輯直接依賴 Unity GameObject 或 MonoBehaviour。

```text
Assets/_Project
├── Scenes
│   └── BattleScene.unity
└── Scripts
    ├── Domain
    ├── ApplicationServices
    │   ├── AI
    │   ├── Calculators
    │   ├── Factories
    │   ├── Formatters
    │   ├── Replay
    │   └── Simulation
    ├── Infrastructure
    │   ├── Queries
    │   └── Records
    └── Presentation
```

| Layer                 | 責任                                           |
| --------------------- | -------------------------------------------- |
| `Domain`              | 核心資料模型與戰鬥狀態                                  |
| `ApplicationServices` | 純 C# 應用邏輯，例如戰鬥模擬、AI、Replay、格式化；不依賴 Unity API |
| `Infrastructure`      | SQLite 儲存與查詢                                 |
| `Presentation`        | Unity UI、場景物件、按鈕事件、畫面更新                      |

---

## 8. 主要程式碼導覽

### 8.1 Domain

Domain 層負責定義核心資料模型與戰鬥狀態，不依賴 Unity UI。

| 路徑                                                               | 說明                                           |
| ---------------------------------------------------------------- | -------------------------------------------- |
| `Assets/_Project/Scripts/Domain/BattleEnums.cs`                  | 定義職業、隊伍、戰鬥結果、畫面狀態                            |
| `Assets/_Project/Scripts/Domain/CharacterAttributes.cs`          | 定義角色初始數值                                     |
| `Assets/_Project/Scripts/Domain/CharacterRuntime.cs`             | 單一角色的 runtime 狀態，例如目前 HP                     |
| `Assets/_Project/Scripts/Domain/TeamRuntime.cs`                  | 單一隊伍的角色集合與查詢方法                               |
| `Assets/_Project/Scripts/Domain/BattleRuntime.cs`                | 一場戰鬥的左右隊伍、回合數、勝負狀態                           |
| `Assets/_Project/Scripts/Domain/BattleSession.cs`                | 一場戰鬥的 session，包含 seed、事件 buffer、AI 套用資訊與儲存狀態 |
| `Assets/_Project/Scripts/Domain/BattleEvent.cs`                  | 單一行動事件資料，用於 Log、SQLite 儲存與 Replay            |
| `Assets/_Project/Scripts/Domain/BattleReplayPayload.cs`          | 從 SQLite 載入的完整 Replay 資料                     |
| `Assets/_Project/Scripts/Domain/BattleReplayInitialCharacter.cs` | Replay 重建角色初始狀態用資料                           |

### 8.2 ApplicationServices

ApplicationServices 層負責純 C# 應用邏輯，例如模擬、AI、格式化與 Replay 控制。此層不依賴 Unity API。

| 路徑                                                           | 說明                                          |
| ------------------------------------------------------------ | ------------------------------------------- |
| `ApplicationServices/Simulation/BattleSimulator.cs`          | 正式戰鬥模擬器，會產生 `BattleEvent`                   |
| `ApplicationServices/Simulation/TargetSelector.cs`           | 攻擊與治療目標選擇邏輯                                 |
| `ApplicationServices/Calculators/DamageCalculator.cs`        | 傷害與治療數值計算                                   |
| `ApplicationServices/Calculators/BattleMetricsCalculator.cs` | 統計資訊計算                                      |
| `ApplicationServices/Factories/BattleSessionFactory.cs`      | 建立隨機或指定排列的戰鬥 session                        |
| `ApplicationServices/Formatters/BattleTextFormatter.cs`      | 職業、隊伍、狀態等顯示文字格式化                            |
| `ApplicationServices/Formatters/BattleLogFormatter.cs`       | 戰鬥事件 Log 文字格式化                              |
| `ApplicationServices/Replay/ReplayController.cs`             | 根據已儲存的 `BattleEvent` event stream 進行 Replay |
| `ApplicationServices/AI/AiTeamOrderEvaluator.cs`             | AI Monte Carlo 評估器，測試左隊 24 種排列              |
| `ApplicationServices/AI/FastBattleOutcomeSimulator.cs`       | AI 專用快速模擬器，只計算勝負，不產生事件                      |
| `ApplicationServices/AI/AiEvaluationResult.cs`               | AI 評估結果資料模型                                 |

### 8.3 Infrastructure

Infrastructure 層負責 SQLite 儲存與查詢。

| 路徑                                                    | 說明                                      |
| ----------------------------------------------------- | --------------------------------------- |
| `Infrastructure/BattlePersistenceService.cs`          | 戰鬥結束後將 `BattleRun`、角色初始狀態與事件資料寫入 SQLite |
| `Infrastructure/BattleSaveResult.cs`                  | 儲存結果資料                                  |
| `Infrastructure/Queries/BattleHistoryQueryService.cs` | 查詢歷史戰鬥摘要，用於回放紀錄列表                       |
| `Infrastructure/Queries/BattleReplayQueryService.cs`  | 讀取完整 Replay payload                     |
| `Infrastructure/Records/BattleRunSummaryRecord.cs`    | 歷史紀錄列表顯示用摘要資料                           |

SQLite 資料庫會建立在 Unity 的 `persistentDataPath` 下，檔名為：

```text
battle_runs.db
```

### 8.4 Presentation

Presentation 層負責 Unity 場景、UI、Button 事件與畫面更新。

| 路徑                                          | 說明                              |
| ------------------------------------------- | ------------------------------- |
| `Presentation/BattleSceneController.cs`     | 主流程控制器，協調戰鬥、Replay、AI、儲存與 UI 狀態 |
| `Presentation/CharacterCardView.cs`         | 單一角色卡顯示與 highlight              |
| `Presentation/CenterControlView.cs`         | 中央按鈕、勝者文字、跳過按鈕控制                |
| `Presentation/MainButtonPanelView.cs`       | 左下主要按鈕區控制                       |
| `Presentation/BattleLogView.cs`             | 戰鬥 Log 顯示                       |
| `Presentation/BattleMetricsView.cs`         | 統計資訊顯示                          |
| `Presentation/LoadListModalView.cs`         | 回放紀錄列表 Modal                    |
| `Presentation/BattleHistoryListItemView.cs` | 回放紀錄列表單列項目                      |
| `Presentation/AiSuggestionModalView.cs`     | AI 計算中與 AI 結果 Modal             |

---

## 9. 戰鬥流程設計

| 步驟 | 說明                             |
| -- | ------------------------------ |
| 1  | 建立左右雙方隊伍                       |
| 2  | 左方先行動                          |
| 3  | 角色依 slot index 依序行動            |
| 4  | HP > 0 的角色才可行動                 |
| 5  | 隨機選擇敵方 HP > 0 的單一角色作為攻擊目標      |
| 6  | 選擇我方 HP > 0 且目前 HP 最低的角色作為治療目標 |
| 7  | 套用傷害與治療                        |
| 8  | 產生 `BattleEvent`               |
| 9  | 檢查勝負                           |
| 10 | 戰鬥結束後一次性寫入 SQLite              |

---

## 10. Replay 設計

Replay 不重新執行隨機邏輯，也不重新計算攻擊目標。Replay 流程會從 SQLite 讀取：

| 資料                       | 用途                                |
| ------------------------ | --------------------------------- |
| `BattleRun`              | 戰鬥基本資訊，例如建立時間、勝者、回合數、seed         |
| `CharacterInitialRecord` | 左右隊伍初始職業、順序、數值                    |
| `BattleEventRecord`      | 每一筆行動事件、HP before / after、攻擊與治療目標 |

Replay 時會依照儲存的 `BattleEvent` 逐筆套用 HP 變化，因此可重現原本戰鬥過程，不受新的 Random 影響。

---

## 11. AI 設計

AI 功能會針對目前右方隊伍配置，測試左方 4 種職業的所有排列。

| 項目       | 說明                                                        |
| -------- | --------------------------------------------------------- |
| 搜尋空間     | 4! = 24 種左隊排列                                             |
| 模擬方式     | Monte Carlo simulation                                    |
| 每種排列模擬場數 | 2,000 場                                                   |
| 總模擬場數    | 48,000 場                                                  |
| 評估目標     | 找出左方勝率最高的排列                                               |
| 隨機策略     | 各排列使用相同 seed sample set，比較更公平                             |
| 執行方式     | `Task.Run` 背景執行，避免阻塞 Unity 主執行緒                           |
| 效能優化     | AI 使用 `FastBattleOutcomeSimulator`，只計算勝負，不產生 Replay event |

AI 結果是機率估計，不保證單場一定勝利。若顯示左方勝率為 60%，代表在大量模擬中左方約有 60% 勝率，單場仍可能輸。

---

## 12. SQLite 儲存策略

本專案採用「戰鬥結束後一次性儲存」策略。

| 時機           | 行為                       |
| ------------ | ------------------------ |
| 戰鬥尚未結束       | 不寫入 SQLite               |
| 戰鬥分出勝負       | 使用 transaction 一次性寫入完整資料 |
| 新的一輪時舊戰鬥尚未結束 | 背景直接模擬舊戰鬥至結束並儲存          |
| Replay       | 不新增新的儲存紀錄                |

此策略避免儲存半場戰鬥造成資料不完整，也方便 Replay 直接依完整 event stream 播放。

---

## 13. AI 協作與開發流程說明

在題目未明確禁止 AI 輔助工具的前提下，本次實作過程中我有使用 AI 作為 pair-programming / implementation assistant，並在此主動說明使用範圍。

AI 主要協助項目包含需求拆解討論、Unity UI workflow 釐清、程式碼草稿產生、除錯方向建議與部分重構建議。實作過程並非將題目一次性交由 AI 產出成品，而是由我逐步確認規格、拆分功能、建立 Unity 場景與 UI、綁定 Inspector references、整合各模組、進行 Play Mode / Standalone Build 測試，並根據測試結果修正功能與互動流程。

本專案的主要架構決策包含 Domain / ApplicationServices / Infrastructure / Presentation 分層、戰鬥事件資料流、SQLite 儲存與 Replay 設計、AI Monte Carlo 模擬策略、背景執行緒化，以及戰鬥演出與跳過流程。這些部分我皆有逐步驗證，並能說明其設計理由與實作方式。

由於我過去主要是後端工程背景，Unity UI 場景拆分與 Inspector 綁定流程是我相對較新的部分，因此有使用 AI 協助確認操作流程與實作方式。最終提交的程式碼、可執行檔、測試驗證與功能取捨由我負責。
