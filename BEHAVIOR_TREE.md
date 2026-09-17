# Behavior Tree 사용법

## 트리 에셋 만들기

1. Unity 메뉴에서 **Window > Behavior Tree**를 엽니다.
2. **New Tree Asset**으로 ScriptableObject 에셋을 만들거나 기존 에셋을 **Tree Asset**에 지정합니다.
3. **Tree Key**를 입력합니다. 비워 두면 선택한 **Target** 게임 오브젝트 이름을 사용합니다.
   에셋에 저장된 트리가 하나뿐이고 선택한 AI의 키가 비어 있다면 에디터는 그 트리의 저장 키를 자동으로 선택합니다. 샘플의 `SampleManager`는 `AI_01` 같은 여러 AI를 모두 `AI` 키로 초기화합니다.
4. 캔버스 우클릭 **Add** 메뉴에는 기본 Action, Condition, Sequence, Selector 노드만 표시됩니다. 사용자 정의 노드와 샘플 노드는 해당 C# 스크립트를 Project 창에서 캔버스로 드래그 앤 드롭하여 추가합니다.
5. 노드를 연결하고 **Save**를 누릅니다. Target을 지정했다면 저장한 에셋과 키가 해당 AI 컴포넌트에도 연결됩니다. **Load**는 선택한 에셋의 해당 키를 엽니다.
6. 노드는 우클릭 **Delete Node** 또는 Delete 키로 제거할 수 있습니다. 배치가 겹치면 **Auto Layout**을 사용합니다.

트리 하나를 여러 AI가 공유할 수 있습니다. 실행 상태는 AI마다 `NodeController.Initialize`가 새 노드를 만들기 때문에 공유되지 않습니다. 기존 `Resources/NodeScriptableObject.asset`과 `AI` 키도 계속 사용할 수 있습니다.

## 사용자 액션

`AI`를 상속한 MonoBehaviour에 아래처럼 공개 인스턴스 메서드를 만들고 ActionNode의 입력칸에 메서드 이름을 적습니다. 이름과 매개변수, 반환 타입이 맞지 않으면 초기화 시 오류가 기록됩니다. 예전의 `HpCheck` 등 7개 메서드는 호환을 위해 남아 있으며 새 AI에서 구현할 필요는 없습니다.

```csharp
public BtState Patrol(bool isFirstTurn)
{
    // 동작 구현
    return BtState.SUCCESS;
}
```

새 AI 컴포넌트는 연결된 트리 에셋을 `Start`에서 자동으로 초기화합니다. 직접 제어하려면 인스펙터에서 자동 초기화를 끄고 `Initialize(key)` 또는 `SetTree(asset, key)`를 호출합니다.

### ActionNode 상속 예제

`Assets/Sample/Script/CooldownActionNode.cs`는 `ActionNode`를 상속합니다. Project 창의 `CooldownActionNode.cs`를 캔버스로 드래그 앤 드롭하고 메서드 이름(예: `Attack`)과 재사용 대기시간(초)을 입력하세요. AI에 `public BtState Attack(bool isFirstTurn)` 메서드가 있어야 합니다. 노드는 기존 `ActionNode`의 실행·자식 처리 로직을 호출하고, 성공 후 설정한 시간 동안 `FAILUER`를 반환합니다. 재사용 대기시간은 에셋에 저장되고 실행 중 남은 상태는 AI 인스턴스마다 독립적입니다.
## 사용자 노드

`Node`를 상속한 `[Serializable]` 공개 클래스를 만들고 `GetState()`를 구현합니다. 공개 기본 생성자가 필요합니다. 설정은 공개 필드나 `[SerializeField]` 필드에 두고, 실행 중 바뀌는 상태는 `[NonSerialized]`로 둡니다. `Reset()`에서 실행 상태를 초기화하세요. 에셋은 노드 타입과 설정을 `SerializeReference`로 저장합니다. 실제 예제는 `Assets/Sample/Script/WaitSecondsNode.cs`입니다.

에디터에서 자체 UI를 그리려면 `#if UNITY_EDITOR` 안에서 `DrawDescription()`을 재정의하고 `rect`를 기준으로 `GUI.Box`, `GUI.Label`, `EditorGUI` 필드를 배치하세요. 드래그는 에디터 창이 노드 상단에서 처리합니다. `GUI.Window`와 `GUI.DragWindow`는 사용하지 마세요. 캔버스 스크롤 영역 밖에 그려지거나 노드가 표시되지 않을 수 있습니다.

새 노드는 플레이어 빌드에 포함되는 런타임 어셈블리에서 컴파일된 뒤 스크립트 드래그 앤 드롭으로 추가합니다. `Editor` 폴더에 두지 마세요. `UnityEngine.Object` 참조를 포함한 설정도 에셋에 저장할 수 있습니다. 관리 코드 스트리핑을 강하게 사용하는 빌드에서는 사용자 노드 타입이 제거되지 않도록 `[UnityEngine.Scripting.Preserve]`를 지정하거나 `link.xml`에 등록하세요.

## 실행 중 노드 보기

Play 모드에서 씬의 AI 오브젝트를 선택하고 **Window > Behavior Tree**를 엽니다. 창이 해당 AI의 트리 에셋과 키를 로드합니다. 현재 `RUNNING`인 가장 깊은 노드에는 움직이는 점과 맥동하는 테두리가 표시되며 그 위치로 스크롤합니다. 마지막 실행 결과가 `SUCCESS`이면 해당 노드를 초록색으로 표시합니다. 틱이 멈춰도 이 강조는 유지됩니다. 실패한 노드는 강조하지 않습니다. 사용자 정의 복합 노드에서 자식을 실행할 때는 `child.Tick()`을 호출해야 자식 실행 상태도 추적됩니다.

실행 중이거나 성공으로 표시할 노드가 없으면 루트 노드로 포커스를 옮기고 보라색 테두리로 표시합니다. 이때 상태 문구는 `ROOT [NO ACTIVE NODE]`이며 실패 색상은 사용하지 않습니다.

최근 실행된 말단 노드는 대표 노드와 별도로 `RUNNING`(노랑), `SUCCESS`(초록) 상태가 표시됩니다. 실패한 검사는 `FAIL` 문구나 빨간색 없이 회색 테두리로 호출 여부만 보여줍니다. 이 표시는 최근 0.75초 동안 호출된 노드에만 나타납니다.

사용자 정의 노드에서 예외가 발생하면 해당 AI의 트리 실행을 멈추고 실패한 노드와 오류를 Console 및 트리 창에 한 번 기록합니다. 문제를 수정한 뒤 `Initialize(key)` 또는 `SetTree(asset, key)`로 다시 시작할 수 있습니다.
