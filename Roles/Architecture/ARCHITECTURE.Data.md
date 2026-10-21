# Data

DataManager.LoadDialogueAsync가 IDialogueProvider.LoadAsync에 데이터 취득을 위임한다. 기본 JsonDialogueProvider가 Story 라벨 TextAsset을 프리로드하고 파싱하며, 제공자 교체 시 데이터 소스 전체를 대체한다. SetDialogueProvider는 기존 캐시를 무효화한다. ReleaseDialogue와 씬 소유 라벨 해제를 구분한다. 저장은 persistentDataPath다. UI에서 파일·Addressables API를 직접 호출하지 않는다.

[전체 구조](../../ARCHITECTURE.md)를 함께 적용한다.
