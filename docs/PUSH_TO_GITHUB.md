# Как загрузить на GitHub

Git репозиторий уже инициализирован и коммит создан!

## Быстрая загрузка (из папки проекта)

```bash
cd "C:\Users\vipde\Downloads\Diffraction-main"

# Убедитесь, что remote добавлен
git remote -v

# Если remote нет, добавьте:
git remote add origin git@github.com:dejar212/Diffraction.git

# Переименуйте ветку в main (если нужно)
git branch -M main

# Загрузите на GitHub
git push -u origin main
```

## Если возникли проблемы с SSH

### Вариант 1: Использовать HTTPS вместо SSH
```bash
git remote remove origin
git remote add origin https://github.com/dejar212/Diffraction.git
git push -u origin main
```

### Вариант 2: Настроить SSH ключ
```bash
# Создайте SSH ключ (если его нет)
ssh-keygen -t ed25519 -C "your_email@example.com"

# Скопируйте публичный ключ
cat ~/.ssh/id_ed25519.pub

# Добавьте ключ в GitHub:
# https://github.com/settings/keys
```

## Что уже сделано:

✅ Git репозиторий инициализирован
✅ Все файлы добавлены
✅ Создан коммит с описанием:
   - Fixed energy conservation (0.00% imbalance)
   - Corrected all energy calculations
   - Added English bat files
   - Added documentation

✅ Remote настроен: `git@github.com:dejar212/Diffraction.git`

## Проверка статуса

```bash
git status
git log --oneline
git remote -v
```

Должно показать:
- Branch: main (или master)
- Commit: "Initial commit: Diffraction solver with energy conservation fixes"
- Remote: origin -> git@github.com:dejar212/Diffraction.git
