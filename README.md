# bm-game-forge-template

Use this template 으로 프로젝트를 생성할 경우, 생성 후

``` bash
git remote add upstream https://github.com/your-org/playforge-template.git
git fetch upstream
git rebase --onto upstream/main --root main
git push -f origin main
```

를 실행하는 것을 권장합니다.

이를 통해 추후 본인의 원격 저장소에 template 의 변경 사항을 반영할 수 있습니다.