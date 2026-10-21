# 데이터

대화 파싱·로드는 IDialogueProvider와 DataManager가 담당합니다. UI에서 파일을 읽지 않습니다. TextAsset/Resources 기반 제공자를 기본으로 사용하여 빌드에서도 로드합니다. 구형 연결 JSON과 표준 배열 입력을 지원하고 빈 데이터·잘못된 형식의 오류를 구분합니다.

DTO의 외부 키는 유지합니다. 캐시는 실제 조회 키에 맞추고 로드/씬 진입 때 중복 항목을 누적하지 않습니다. 저장 경로는 Application.persistentDataPath입니다. 소스 Assets를 실행 중 덮어쓰지 않습니다. 제공자 교체 API는 테스트나 실제 교체 요구가 있을 때만 만듭니다.
