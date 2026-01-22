# Vision Lingo Client

Apple Vision Pro를 위한 Unity 기반의 VR 어플리케이션 프로젝트입니다.

---

## 1. Environment

### Unity Version
- **Version**: 6000.0.58f1

### Target Platform
- **Device**: Apple Vision Pro
- **OS Version**: visionOS 2.4.1
- **RenderPipeline**: Universal Render Pipeline (URP)
### Key Packages
- **com.unity.xr.visionos**: 2.3.1
- **com.unity.xr.hands**: 1.5.1
- **com.unity.xr.interaction.toolkit**: 3.0.8
- **com.unity.render-pipelines.universal**: 17.0.4
- **com.unity.inputsystem**: 1.14.2

---

## 2. Project Structure

### Root `Assets` Structure
*   `Scene`: Contains sub-folders for development environments.
    *   `Dev`: Scenes for PC debugging (Simulator/Editor).
    *   `VisionPro`: Production scenes optimized for the device.
*   `Script`: Source code organized by architecture layer (`Controller`, `System`, `UI`, `Component` etc.).
*   `Art`: Art assets including textures, materials, and models.
*   `Prefab`: Reusable game objects for UI, Interactors, and Environment logic.
*   `ExternalResources`: Imported third-party assets and fonts.
*   `XR`: XR-specific configuration settings and profiles.

---

## 3. Development Workflow & Methodology

### Dual-Scene Development Architecture

본 프로젝트는 Apple Vision Pro 기기가 없는 환경에서도 원활한 개발 및 테스트가 가능하도록 **이원화된 씬(Scene) 관리 구조**를 채택했습니다.

#### 1. Development Mode (`_Dev` Logic)
*   **Scene Naming Convention**: `SceneName_Dev` (e.g., `Lobby_Dev`, `Tutorial_Dev`).
*   **Purpose**: PC(Editor/Simulator) 환경에서의 빠른 로직 검증 및 디버깅.
*   **Target Audience**: Vision Pro 기기를 보유하지 않은 개발자.
*   **Logic Implementation**:
    *   `MainSystem.Instance.IsDev` 플래그(Inspector 설정)에 따라 `SceneLoader`가 자동으로 적절한 씬을 로드합니다.
    *   `SceneLoader.cs`는 `LoadLobby()` 호출 시 `IsDev`가 `true`이면 `Lobby_Dev`를, `false`이면 `Lobby`를 로드하도록 분기 처리되어 있습니다.

#### 2. Production Mode (Vision Pro Logic)
*   **Scene Naming Convention**: Standard Name (e.g., `Lobby`, `Tutorial`).
*   **Purpose**: 실제 기기 빌드 및 최종 퀄리티 검증.
*   **Target Audience**: Vision Pro 기기를 보유한 개발자 및 빌드 머신.
*   **Logic Implementation**:
    *   `MainSystem` 프리팹 혹은 씬 내 `MainSystem` 객체의 `IsDev` 체크박스를 `false`로 설정하여 빌드합니다.
    *   Metal Rendering 및 VisionOS 전용 기능들이 활성화된 상태로 동작합니다.

### Code-Level Scene Management
*   **`MainSystem.cs`**: 전역 시스템 관리자로서 `IsDev` 변수를 관리합니다.
*   **`SceneLoader.cs`**: 씬 전환 요청을 중재하며, `IsDev` 값에 따라 동적으로 타겟 씬 이름을 결정합니다.
*   **`Bootstrap` Scene**: 앱 실행 시 가장 먼저 로드되는 진입점(Entry Point)으로, 여기서 `MainSystem`이 초기화되고 환경에 맞는 첫 번째 씬(`LoadLobby` or `LoadTutorial`)으로 분기합니다.

```csharp
// Example Logic in SceneLoader.cs
public void LoadLobby() => LoadScene(MainSystem.Instance.IsDev ? "Lobby_Dev" : "Lobby");
```

---

## 4. App Mode: Metal API (VR)

본 프로젝트는 Apple Vision Pro의 **Metal Rendering with Compositor Services (Metal 모드)** 를 기반으로 개발되었습니다. RealityKit 모드(PolySpatial) 대신 Metal API를 선택한 주요 이유는 다음과 같습니다.

### 선정 이유

1.  **공간 오디오 (Spatial Audio) 지원**
    *   RealityKit 기반의 PolySpatial 모드에서는 Unity의 오디오 기능을 온전히 사용하는 데 제약이 존재합니다.
    *   Metal 모드를 사용함으로써 Unity의 Audio system과 공간 오디오 기능을 완벽하게 지원하며, 몰입감 있는 사운드 환경을 제공합니다.

2.  **원활한 렌더링 및 그래픽 제어**
    *   Metal API를 직접 활용하여 GPU 성능을 최대로 이끌어내며, RealityKit 레이어를 거치지 않아 오버헤드가 적고 더 안정적인 프레임레이트를 확보할 수 있습니다.
    *   **커스텀 쉐이더 호환성**: RealityKit 변환 과정에서 일부 쉐이더가 깨지거나 작동하지 않는 문제를 방지하고, Unity의 쉐이더 그래프 및 커스텀 쉐이더를 그대로 사용할 수 있습니다.
    *   전체적으로 더 깔끔하고 의도한 대로 렌더링 품질을 보장합니다.

3.  **Post Processing 완벽 지원**
    *   Unity의 URP(Universal Render Pipeline) 기반 Post Processing Stack(Bloom, Color Grading 등)을 제한 없이 사용할 수 있어, 시각적 완성도를 높일 수 있습니다.

4.  **HDR (High Dynamic Range) 지원**
    *   Vision Pro의 뛰어난 디스플레이 성능을 활용하기 위해 HDR 렌더링을 지원하여 더 풍부한 색감과 명암비를 표현합니다.

---

## 5. 시선 기반 인터렉션 (Gaze-based Interaction)

본 프로젝트는 손 제스처가 아닌, **사용자의 시선(Head Gaze)과 체류 시간(Dwell Time)** 을 기반으로 한 인터렉션 시스템을 채택했습니다.

### 구현 이유

1.  **사용성 개선 (접근성)**
    *   기존의 제스처(Pinch, Gaze & Pinch) 기반 인터렉션은 어린 아이나 VR 기기에 익숙하지 않은 사용자에게 진입 장벽이 높았습니다.
    *   제스처 인식 실패나 오동작(예외 상황)이 빈번하게 발생하여 사용자 경험을 저해하는 요소가 되었습니다.
    *   직관적으로 "바라보는 것"만으로 상호작용이 가능하도록 하여 누구나 쉽게 사용할 수 있게 개선하였습니다.

2.  **기술적 제약 사항 극복**
    *   Unity의 **Metal App Mode (Immersive Space)** 에서는 개인정보 보호 정책 등으로 인해 실시간 눈동자 추적(Eye Tracking) 데이터에 직접 접근하는 API가 제한(Block)되어 있습니다.
    *   이에 따라 Vision Pro의 정밀한 아이트래킹을 사용하는 대신, **HMD의 정면 벡터(Head Forward)** 를 활용한 유사 시선 추적 방식을 구현하여 이에 대응하였습니다.

### 세부 구현 내용 (`XRHeadRayInteractor`)

*   **Raycasting 매커니즘**:
    *   `Camera.main` (HMD)의 정면 방향(`transform.forward`)으로 Ray를 발사하여 오브젝트를 탐지합니다.
    *   Vision Pro 핸드 트래킹의 핀치 제스처 시 포인터 입력이 튀는 현상을 방지하기 위해, 컨트롤러 포인터 대신 HMD 자체의 회전값을 기준으로 좌표를 계산합니다.

*   **인터렉션 로직**:
    *   `IXRHeadInteractable` 인터페이스를 상속받은 객체(`IsInteractable` 체크)에 대해 상호작용을 수행합니다.
    *   **Dwell Time (체류 시간) 시스템**:
        *   **Wait Time (`_waitRayTime`)**: 사용자가 오브젝트를 실수로 스쳐 지나갈 때 쿨타임을 두어 오작동을 방지합니다 (기본 0.5초).
        *   **Fill Time (`_rayTime`)**: 일정 시간 이상 바라보고 있으면 게이지가 차오르며, 완료 시 `OnSelect()` 이벤트가 발생합니다.
    *   **쿨타임 관리**: 상호작용 후 `_rayCooltime` 동안 추가 입력을 막아 중복 실행을 방지합니다.

*   **피드백**:
    *   튜토리얼 등 특정 상황에서 잘못된 선택을 할 경우 UI 흔들림(`ShakeUI`) 효과 등을 통해 직관적인 피드백을 제공합니다.

---

## 6. 세부 구현 사항

### 팩토리 메서드 기반 팝업 관리 시스템 (`UIPanelFactory`)

전역적인 UI 팝업 및 메시지 관리를 위해 **팩토리/싱글톤 패턴**을 활용한 관리 시스템을 구축했습니다.

*   **중앙 집중식 관리**:
    *   `UIPanelFactory.Instance`를 통해 어디서든 팝업을 호출할 수 있습니다.
    *   `ShowMessage`, `ShowLastMessage` 등의 메서드로 일관된 UI 생성 인터페이스를 제공합니다.

*   **생명주기 및 씬 관리**:
    *   씬 전환 시 `SceneLoader`와 연동하여 자동으로 잔여 팝업을 정리(`ClearPopup`)하고 UI 진행 상태를 초기화합니다.
    *   `DontDestroyOnLoad`를 통해 씬이 변경되어도 매니저가 유지되도록 설계되었습니다.

*   **일시정지 연동**:
    *   `MainSystem`의 `Act_Pause`, `Act_Resume` 이벤트에 자동으로 구독/해지되어, 게임 일시정지 시 팝업의 타이머나 애니메이션도 함께 제어됩니다.

