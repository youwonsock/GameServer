# Network 2D Game

Unity 클라이언트와 C# 게임 서버를 연동해 로그인, 실시간 전투, 인벤토리, DB 저장, 관심 영역 기반 동기화를 구현한 2D 멀티플레이 포트폴리오 프로젝트입니다.

## 목차

- [프로젝트 개요](#프로젝트-개요)
- [프로젝트 요약](#프로젝트-요약)
- [사용 기술](#사용-기술)
- [클래스 구조 UML](#클래스-구조-uml)
- [기능 상세](#기능-상세)
- [빌드 및 실행](#빌드-및-실행)

## 프로젝트 개요

| 항목 | 내용 |
| --- | --- |
| 개발 인원 | 1명 — 유원석 (You Won Sock) |
| GitHub | [youwonsock](https://github.com/youwonsock) |
| 이메일 | qazwsx233434@gmail.com |
| 개발 기간 | 2024.08 ~ 2024.10 |
| 프로젝트 목적 | Unity 기반 2D 멀티플레이 게임과 서버 권위형 게임 로직·DB 연동 구조 구현 |
| 개발 언어 | C# |
| 클라이언트 | Unity 2019.3.15f1 |
| 서버 | .NET 8, ASP.NET Core |
| 네트워크 | TCP Socket, SocketAsyncEventArgs, Google Protocol Buffers |
| 데이터베이스 | Microsoft SQL Server LocalDB, Entity Framework Core 8.0.8 |
| 주요 라이브러리 | Google.Protobuf 3.28.2, Newtonsoft.Json 13.0.3 |

## 프로젝트 요약

Network 2D Game은 계정 서버, 게임 서버, Unity 클라이언트를 분리하고 서로 다른 통신 목적에 맞춰 REST API와 TCP 소켓을 함께 사용한 프로젝트입니다. 게임 서버가 이동·전투·몬스터·보상·인벤토리 상태를 처리하고, 클라이언트는 서버에서 받은 결과를 화면에 반영합니다.

주요 실행 흐름은 다음과 같습니다.

1. Unity 클라이언트가 ASP.NET Core AccountServer에 계정 생성 또는 로그인 요청을 전송합니다.
2. 로그인 성공 후 클라이언트가 TCP 7777 포트의 GameServer에 연결합니다.
3. 패킷은 크기와 ID를 포함한 헤더 및 Protocol Buffers 메시지로 직렬화됩니다.
4. 수신 작업은 GameRoom, DB, 클라이언트 메인 스레드의 JobQueue로 전달되어 순차 처리됩니다.
5. 게임 서버가 이동, 전투, 몬스터 AI, 아이템 보상과 캐릭터 상태를 관리합니다.
6. Zone과 VisionCube가 플레이어의 관심 영역을 계산하고 필요한 오브젝트만 동기화합니다.

## 사용 기술

### Unity Client

로그인, 캐릭터 선택, 게임 플레이, 인벤토리 UI를 구성했습니다. `NetworkManager`가 게임 서버 연결과 패킷 송신을 담당하고, 네트워크 스레드에서 받은 메시지는 `PacketQueue`를 거쳐 Unity 메인 스레드에서 처리합니다.

### ServerCore

`Listener`, `Connector`, `Session`, `RecvBuffer`로 비동기 TCP 통신 계층을 구성했습니다. `SocketAsyncEventArgs`를 사용해 송수신을 처리하고, 길이 기반 패킷 프레이밍으로 분할되거나 연속해서 도착한 패킷을 구분합니다.

### Game Server

`GameLogic`과 `GameRoom`이 게임 월드와 오브젝트의 생명주기를 관리합니다. 게임 로직, 네트워크 송신, DB 작업을 분리하고 `JobSerializer`와 `JobTimer`로 작업 순서와 지연 실행을 제어합니다.

### Account Server / Database

ASP.NET Core REST API로 계정 생성과 로그인을 처리합니다. Entity Framework Core와 SQL Server LocalDB를 사용해 계정, 캐릭터 스탯, 아이템 및 장착 상태를 영속화합니다.

### Protocol Buffers / PacketGenerator

Protocol Buffers로 클라이언트와 서버가 공유하는 메시지 형식을 정의했습니다. `PacketGenerator`가 패킷 ID 등록과 메시지 처리 코드를 생성해 양쪽의 패킷 처리 규칙을 일치시킵니다.

## 클래스 구조 UML

핵심 모듈의 관계는 다음과 같습니다.

- `NetworkManager` → `ServerSession`, `PacketQueue`, `PacketManager`
- `Session` → `PacketSession` → Client/Server 전용 세션
- `GameLogic` → `GameRoom` → `Player`, `Monster`, `Projectile`
- `GameRoom` → `Map`, `Zone`, `VisionCube`
- `DbTransaction` → Entity Framework Core → SQL Server LocalDB

> **UML 플레이스홀더** — Unity Client, AccountServer, GameServer와 핵심 클래스의 관계를 나타내는 전체 구조 UML 추가 예정

### 클래스별 역할

- `Session`: 비동기 소켓 연결, 송수신 큐, 연결 종료와 수신 버퍼를 관리합니다.
- `PacketSession`: 패킷 길이 헤더를 해석하고 완성된 패킷을 상위 처리기로 전달합니다.
- `NetworkManager`: Unity 클라이언트의 게임 서버 연결과 메인 스레드 패킷 처리를 조정합니다.
- `ClientSession`: 서버에서 접속 상태, 계정, 캐릭터와 플레이어 세션을 관리합니다.
- `GameLogic`: 게임 룸의 생성과 주기적인 업데이트 작업을 관리합니다.
- `GameRoom`: 게임 오브젝트의 입장·이동·전투·퇴장 및 패킷 브로드캐스트를 처리합니다.
- `JobSerializer`: 여러 스레드에서 전달된 작업을 큐에 저장하고 정해진 실행 지점에서 순차 처리합니다.
- `Map`: 충돌 맵, 오브젝트 위치와 격자 기반 경로 탐색을 담당합니다.
- `Zone`: 공간별 Player, Monster, Projectile 집합을 관리합니다.
- `VisionCube`: 플레이어 주변의 현재 오브젝트와 이전 오브젝트를 비교해 Spawn/Despawn 대상을 계산합니다.
- `DbTransaction`: 게임 상태 저장과 아이템 보상 DB 작업을 별도 큐에서 처리한 뒤 결과를 게임 룸에 반영합니다.

## 기능 상세

### 계정 생성 및 로그인

**목적**

게임 접속 전에 계정을 생성하고 인증한 뒤 게임 서버 연결 단계로 전환합니다.

**핵심 구현**

- ASP.NET Core AccountServer가 `account/create`, `account/login` POST 요청을 처리합니다.
- Entity Framework Core로 계정 중복 여부와 로그인 정보를 조회합니다.
- Unity의 `WebManager`가 JSON 요청을 전송하고 성공 결과를 UI에 반영합니다.
- 인증 성공 후 `NetworkManager`가 GameServer에 TCP 연결을 시작합니다.

![계정 생성 및 로그인](https://github.com/user-attachments/assets/a919d285-0b14-461e-8a8a-991db1f6eba0)

### 비동기 TCP 통신 및 패킷 처리

**목적**

클라이언트와 게임 서버가 연결을 유지하면서 여러 종류의 게임 패킷을 비동기로 교환합니다.

**핵심 구현**

- `SocketAsyncEventArgs` 기반의 비동기 accept, connect, send, receive 흐름을 구성했습니다.
- 수신 버퍼에 누적된 데이터를 `[size(2)][packetId(2)][payload]` 형식으로 분리합니다.
- Protocol Buffers 메시지를 패킷 ID별 handler에 연결합니다.
- 클라이언트는 수신 패킷을 `PacketQueue`에 저장한 뒤 Unity 메인 스레드에서 처리합니다.

> **UML 플레이스홀더** — 소켓 수신부터 패킷 프레이밍, `PacketQueue`, Unity 메인 스레드 처리까지의 시퀀스 UML 추가 예정

### JobQueue 기반 작업 직렬화

**목적**

네트워크, 게임 로직, DB 작업이 공유 상태를 동시에 변경하지 않도록 실행 순서를 제어합니다.

**핵심 구현**

- `JobSerializer`가 다른 스레드에서 요청된 작업을 잠금으로 보호된 큐에 저장합니다.
- `GameLogic`, `GameRoom`, `DbTransaction`이 각자의 큐를 순차적으로 비웁니다.
- `JobTimer`가 몬스터 AI와 VisionCube 갱신 같은 지연 작업을 예약합니다.
- DB 처리 완료 후 결과 작업을 다시 GameRoom 큐에 전달해 게임 상태에 반영합니다.

> **코드 샘플 플레이스홀더** — `Push`, `PushAfter`, `Flush`와 DB 완료 후 GameRoom으로 작업을 반환하는 흐름을 보여주는 실행 가능한 예제 추가 예정

### 데이터베이스 연동

**목적**

계정, 캐릭터 스탯과 아이템 정보를 저장하고 재접속 후에도 게임 상태를 복원합니다.

**핵심 구현**

- Entity Framework Core로 Account, Player, Item 모델과 관계를 구성했습니다.
- 계정명과 캐릭터명에 unique index를 적용했습니다.
- 캐릭터 입장 시 스탯과 보유 아이템을 조회해 서버 오브젝트를 초기화합니다.
- 체력 저장과 아이템 보상 처리는 `DbTransaction` 큐에서 실행합니다.

![데이터베이스 계정 정보](https://github.com/user-attachments/assets/c7683922-4a7d-4814-af1e-f9d49207a5dd)
![데이터베이스 캐릭터 및 아이템 정보](https://github.com/user-attachments/assets/d004407a-e17a-4699-861f-c86c19a08690)

### 스탯 및 인벤토리

**목적**

플레이어의 장비 변경과 보상 획득 결과를 서버 기준으로 관리합니다.

**핵심 구현**

- Protocol Buffers의 `StatInfo`, `ItemInfo`로 스탯과 아이템 상태를 공유합니다.
- 장착·해제 요청을 서버가 검증하고 변경된 스탯과 장착 상태를 DB에 저장합니다.
- 몬스터 처치 보상은 빈 슬롯을 확인한 뒤 DB 저장 성공 시 인벤토리에 추가합니다.
- 변경된 아이템 목록과 스탯을 패킷으로 클라이언트에 전달합니다.

![스탯 및 인벤토리](https://github.com/user-attachments/assets/0de5e876-cca5-4e93-a08c-1262d66dd74a)

### 공간 분할 및 관심 영역 동기화

**목적**

모든 오브젝트 정보를 전체 플레이어에게 전송하지 않고 주변 오브젝트만 동기화합니다.

**핵심 구현**

- `GameRoom`이 맵을 Zone 단위로 분할합니다.
- 오브젝트가 Zone 경계를 넘으면 기존 집합에서 제거하고 새 Zone에 등록합니다.
- `VisionCube`가 인접 Zone에서 시야 범위 안의 오브젝트를 수집합니다.
- 이전 결과와 현재 결과의 차집합으로 Spawn/Despawn 패킷을 생성합니다.
- 플레이어 이동 패킷도 위치를 기준으로 필요한 범위에만 브로드캐스트합니다.

![공간 분할 및 관심 영역](https://github.com/user-attachments/assets/88cc95ad-bd12-4fdc-b3e3-98416ad679b6)

### 격자 기반 경로 탐색

**목적**

서버에서 충돌과 점유 상태를 고려해 몬스터가 플레이어에게 이동할 경로를 계산합니다.

**핵심 구현**

- 충돌 맵과 오브젝트 점유 배열을 사용해 이동 가능한 셀을 검사합니다.
- PriorityQueue와 open/closed 집합, parent 기록으로 탐색 후보를 관리합니다.
- 목적지에 도달하지 못하면 탐색한 셀 중 목적지와 가장 가까운 위치까지의 경로를 반환합니다.
- 계산한 다음 셀로 서버의 몬스터 위치를 갱신하고 이동 패킷을 주변 플레이어에게 전송합니다.

![격자 기반 경로 탐색](https://github.com/user-attachments/assets/af6c9ac6-a7ee-4e93-8a52-28b198ae93f5)

## 빌드 및 실행

### 요구 환경

- Unity 2019.3.15f1
- Visual Studio 2022 또는 .NET 8 SDK
- Microsoft SQL Server Express LocalDB
- Windows 환경

### GameServer

`Server/Server/Server.csproj`를 빌드하고 실행합니다.

```powershell
dotnet restore ".\Server\Server\Server.csproj"
dotnet run --project ".\Server\Server\Server.csproj"
```

GameServer는 기본적으로 로컬 호스트의 TCP 7777 포트에서 연결을 대기하며, `GameDB` LocalDB와 `Common/MapData`의 맵 데이터를 사용합니다.

### AccountServer

AccountServer는 HTTPS REST API를 제공하며 Unity 클라이언트의 기본 접속 주소는 `https://localhost:5001/api`입니다. 현재 저장소에는 `Server/AccountServer/AccountServer.csproj`가 포함되어 있지 않으므로 실행하려면 ASP.NET Core 프로젝트 설정과 NuGet 참조를 복원해야 합니다.

### Unity Client

Unity Hub에서 `Client` 폴더를 Unity 2019.3.15f1 프로젝트로 열고 `Assets/Scenes/Login.unity` 씬을 실행합니다. 로그인 성공 후 클라이언트가 AccountServer와 GameServer에 순서대로 연결합니다.

> **참고** — 이 저장소는 포트폴리오 소스 열람을 중심으로 정리되어 있습니다. `Client/Packages`와 일부 서버 프로젝트 파일이 포함되어 있지 않아 원본 실행 환경을 완전히 재현하려면 누락된 프로젝트 설정을 복원해야 합니다.
