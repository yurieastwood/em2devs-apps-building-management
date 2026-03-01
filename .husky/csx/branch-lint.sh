#!/bin/sh

branch=$(git rev-parse --abbrev-ref HEAD)

# Allow well-known default branches
if echo "$branch" | grep -qE "^(main|master|develop|dev)$"; then
  exit 0
fi

# Conventional Branches pattern: type/description
pattern="^(main|feat|fix|hotfix|release|chore)/.+"

if ! echo "$branch" | grep -qE "$pattern"; then
  echo ""
  echo "ERROR: Invalid branch name."
  echo ""
  echo "  Got: $branch"
  echo ""
  echo "  Expected: <type>/<description>"
  echo ""
  echo "  Valid types: main, feat, fix, hotfix, release, chore"
  echo ""
  echo "  Examples:"
  echo "    feat/user-registration"
  echo "    fix/login-timeout"
  echo "    chore/update-dependencies"
  echo ""
  exit 1
fi
