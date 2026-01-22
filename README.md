# 온소리 Sphere: Vision Pro 기반 공간청각·감각통합 XR 청능재활 시스템

[![Unity](https://img.shields.io/badge/Unity-6000.0.58f1-black.svg?style=flat&logo=unity)](https://unity.com/releases/editor/whats-new/6000.0.58f1)
[![Platform](https://img.shields.io/badge/platform-visionOS_2.4.1-lightgrey?logo=apple)](https://developer.apple.com/visionos/)
[![Pipeline](https://img.shields.io/badge/RenderPipeline-URP-orange)]()

## 📝 Introduction
이 프로젝트는 **Apple Vision Pro**를 위한 Unity 기반의 VR 어플리케이션으로, **Metal Rendering with Compositor Services**를 활용하여 고품질의 공간 오디오와 시각적 몰입감을 제공하는 청능재활 및 감각통합 시스템입니다.

---

## 🛠 Development Environment

### Core Requirements
Unity 프로젝트 버전 호환성 및 Vision Pro 빌드를 위해 아래 명시된 환경을 권장합니다.

| Component | Version | Details |
| :--- | :--- | :--- |
| **Unity Editor** | `6000.0.58f1` | (필수) visionOS 빌드 지원 버전 |
| **Target Platform** | Apple Vision Pro | visionOS 2.4.1 |
| **Render Pipeline** | URP | Universal Render Pipeline |
| **App Mode** | Metal (VR) | RealityKit(PolySpatial) 미사용 |

### Dependencies & Packages
이 프로젝트는 다음의 핵심 패키지들을 사용합니다.

| Package Name | Version | Note |
| :--- | :--- | :--- |
| **com.unity.xr.visionos** | `2.3.1` | VisionOS 플랫폼 지원 |
| **com.unity.xr.hands** | `1.5.1` | 핸드 트래킹 |
| **com.unity.xr.interaction.toolkit** | `3.0.8` | XRI 상호작용 시스템 |
| **com.unity.render-pipelines.universal** | `17.0.4` | 렌더링 파이프라인 |
| **com.unity.inputsystem** | `1.14.2` | 입력 시스템 |

---

## 📂 Project Structure

> **📝 Naming Convention Rule**
> * 구조 외의 파일 네이밍은 자유롭게 가능합니다.
> * 단, 순서도가 있는 항목(Scene 등)은 `숫자_` prefix를 사용하여 정렬합니다.
> * 그 외에는 자유롭게 작성하되, 팀 내 합의된 규칙을 따릅니다.

### 🎨 Art
아트 리소스들을 담습니다.

> **📂 상세 구조**
> * **Animation**
>   * `Animation`: 작동하는 Animation 파일을 담는다. (`.anim`)
>   * `Animator`: Animation을 동작 시키는 Animator를 담는다. (`.animator`)
> * **Font**: Font 파일을 담는다. (`.ttf`, `*SDF.asset`)
> * **Material**: Material 파일을 담는다. (`.mat`)
> * **Mesh**: 3D mesh 파일을 담는다. (`.fbx`)
> * **Settings**: URP Graphics Setting 파일들을 담는다. (`.asset`)
> * **ShaderGraph**: ShaderGraph 파일들을 담는다. (`.shadergraph`)
> * **Sound**
>   * `Music`: 음악 파일을 담는다. (`.mp3`, `.wav` 등)
>   * `SFX`: 효과음 파일을 담는다. (`.mp3`, `.wav` 등)
> * **Sprite**: Sprite로 변환된 파일들을 담는다. (`.jpg`, `.png` + `.meta`)
> * **Texture**: Sprite로 변환되지 않는 이미지 파일들을 담는다. (`.jpg`, `.png`)

### 📦 ExternalResources
외부 라이브러리를 담습니다.
* *예시: Newtonsoft JSON, UniRX*

### 🧱 Prefab
재사용 가능한 GameObject를 담습니다.

> **📂 상세 구조**
> * **Popup**: (런타임 후) 재사용되는 Popup을 담는다. (*예: `Popup_Setting.prefab`, `Popup_Noti.prefab`*)
> * **VFX**: 이펙트 프리팹을 담는다. (*예: `Spark.prefab`, `Starlight.prefab`*)
> * **UI**: (런타임 전) 재사용되는 UICanvas와 UI를 담는다.

### 🗂 Resources
런타임에 Asset을 불러오기 위한 폴더입니다. (권장되지 않음)
* *예시: `Sound/sfx/sfx_01.mp3`*

### 🎬 Scene
Scene 파일들을 담습니다. (*순서가 필요한 경우 숫자 Prefix 사용*)

### 📜 Script
소스 코드를 담습니다.

> **📂 상세 구조**
> * **Editor**: Editor 환경에서만 작동하는 스크립트들을 담는다.
> * **Runtime**
>   * `Component`: 재사용 가능한 Prefab의 스크립트들을 담는다.
>   * `Controller`: 하나의 Scene에서 개별적으로 동작하는 요소들을 넣는다.
>   * `System`: 하나의 Scene을 관장하는 스크립트나 전역으로 관리되는 스크립트들을 담는다.
>   * `UI`: UI 동작 관련 스크립트들을 담는다.

### 💾 ScriptableObject
ScriptableObject 데이터를 담습니다 (`.asset`).

### 🧪 TestFiles
Git에 올리지 않을 Local Test File들을 담습니다. (Feature 브랜치엔 업로드 가능, Main 병합 시 삭제)
* *예시: `CYArt/Script`, `JSArt/Script`*

### 📄 Text
텍스트 파일을 담습니다 (`.json`, `.xml` 등).

---

## 📊 System Architecture & Methodology

### 1. Dual-Scene Development Architecture
Apple Vision Pro 기기 없이도 원활한 개발이 가능하도록 **이원화된 씬(Scene) 관리 구조**를 채택했습니다.

| Mode | Scene Naming | Purpose | Logic |
| :--- | :--- | :--- | :--- |
| **Dev Mode** | `SceneName_Dev` | PC(Editor) 내 로직 검증 | `MainSystem.Instance.IsDev = true` 시 로드 |
| **Prod Mode** | `SceneName` | 실기기 빌드 및 최종 검증 | `IsDev = false` 설정, Metal/VisionOS 기능 활성화 |

* **Logic Implementation**: `SceneLoader.cs`는 `MainSystem`의 `IsDev` 플래그에 따라 타겟 씬(`Lobby` vs `Lobby_Dev`)을 동적으로 결정하여 로드합니다.

### 2. Metal API (VR) Implementation
RealityKit(PolySpatial) 대신 **Metal Rendering** 모드를 사용하여 다음 기능을 극대화했습니다.

* **Spatial Audio**: Unity Audio System 완전 호환을 통한 고도화된 공간 음향 제공.
* **Rendering Control**: RealityKit 레이어 오버헤드 제거를 통한 안정적 프레임 및 커스텀 쉐이더 호환성 확보.
* **Post Processing**: URP 기반의 Bloom, Color Grading 등 후처리 효과 제한 없이 사용.
* **HDR**: Vision Pro 디스플레이 성능을 활용한 High Dynamic Range 렌더링 지원.

### 3. Gaze-based Interaction
접근성 향상 및 오동작 방지를 위해 **시선(Head Gaze) + 체류 시간(Dwell Time)** 방식을 사용합니다.
#### 구현 이유

1.  **사용성 개선 (접근성)**
    *   기존의 제스처(Pinch, Gaze & Pinch) 기반 인터렉션은 어린 아이나 VR 기기에 익숙하지 않은 사용자에게 진입 장벽이 높았습니다.
    *   제스처 인식 실패나 오동작(예외 상황)이 빈번하게 발생하여 사용자 경험을 저해하는 요소가 되었습니다.
    *   직관적으로 "바라보는 것"만으로 상호작용이 가능하도록 하여 누구나 쉽게 사용할 수 있게 개선하였습니다.

2.  **기술적 제약 사항 극복**
    *   Unity의 **Metal App Mode (Immersive Space)** 에서는 개인정보 보호 정책 등으로 인해 실시간 눈동자 추적(Eye Tracking) 데이터에 직접 접근하는 API가 제한(Block)되어 있습니다.
    *   이에 따라 Vision Pro의 정밀한 아이트래킹을 사용하는 대신, **HMD의 정면 벡터(Head Forward)** 를 활용한 유사 시선 추적 방식을 구현하여 이에 대응하였습니다.
#### 구현 내용
* **Raycasting**: `Camera.main` (HMD)의 정면 벡터(`transform.forward`)를 기준으로 Raycasting 수행.
* **Dwell Time**:
    * **Wait Time**: 실수로 스쳐 지나가는 입력을 방지하기 위한 대기 시간.
    * **Fill Time**: 일정 시간 응시 시 게이지가 차오르며 `OnSelect()` 이벤트 발생.
* **Feedback**: 잘못된 선택 시 UI 흔들림(`ShakeUI`) 효과 등으로 직관적인 피드백 제공.

### 4. UI System (Factory Pattern)
* **Structure**: `UIPanelFactory` (Singleton)를 통한 중앙 집중식 팝업 관리.
* **Lifecycle**: 씬 전환 시 `SceneLoader`와 연동하여 잔여 팝업 자동 정리(`ClearPopup`).
* **Sync**: 시스템 일시정지(`Act_Pause`) 시 UI 애니메이션 및 타이머 동기화.


