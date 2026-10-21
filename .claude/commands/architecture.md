# Architecture review

CLAUDE.md, ARCHITECTURE.md, 관련 Roles/Architecture 문서를 읽고 현재 브랜치의 `git diff main...HEAD` 및 작업 트리 변경을 검토합니다. main 자체에서는 비교 대상을 명시합니다. 없는 dev 브랜치를 전제하지 않습니다.

Facade 등록/DI 순환, Init/Release 소유권, State/Screen 분리, 입력/이벤트 경계, 씬·GUID·직렬화 계약, Resources 키와 빌드 씬 경로를 검사합니다. 실제 변경 위치와 재현 가능한 영향을 보고합니다. 존재하지 않는 Chalk 매니저나 브리지를 요구하지 않습니다. 검토 요청만으로 파일을 수정하거나 커밋하지 않습니다.
