#!/bin/sh

commit_msg_file="$1"
commit_msg=$(head -1 "$commit_msg_file")

# Allow standard Git-generated messages
if echo "$commit_msg" | grep -qE "^(Merge |Revert |Initial commit|fixup! |squash! )"; then
  exit 0
fi

# Conventional Commits pattern: type(optional-scope)[!]: description
pattern="^(feat|fix|build|chore|ci|docs|style|refactor|perf|test)(\(.+\))?!?: .{1,}"

if ! echo "$commit_msg" | grep -qE "$pattern"; then
  echo ""
  echo "ERROR: Invalid commit message format."
  echo ""
  echo "  Got: $commit_msg"
  echo ""
  echo "  Expected: <type>[optional scope]: <description>"
  echo ""
  echo "  Valid types: feat, fix, build, chore, ci, docs, style, refactor, perf, test"
  echo ""
  echo "  Examples:"
  echo "    feat: add user registration"
  echo "    fix(auth): resolve token expiration bug"
  echo "    chore: update dependencies"
  echo ""
  exit 1
fi
