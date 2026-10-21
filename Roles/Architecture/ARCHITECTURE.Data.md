# Data

DataManager가 Resources TextAsset을 IDialogueProvider에 전달하고 씬·단계에 맞는 대사를 제공합니다. 연결 JSON/배열 형식과 기존 외부 키를 지원합니다. 저장 데이터는 persistentDataPath로 분리합니다. ResourceManager는 이름으로 검색할 프리팹·스프라이트를 캐시합니다. 실제로 사용하는 Resources 계약을 유지하며 Addressables는 추후 별도 도입 사항입니다.

[전체 구조](../../ARCHITECTURE.md)와 해당 [컨벤션](../Convention/UNITY_CODING_CONVENTION.md)을 함께 적용합니다.
