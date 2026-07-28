import re

for file_path in ['src/YAERP.Domain/Entities/Security/ApplicationRole.cs', 'src/YAERP.Domain/Entities/Security/ApplicationUser.cs']:
    with open(file_path, 'r') as f:
        content = f.read()

    # Remove IAuditableEntity if it doesn't exist
    content = re.sub(r', IAuditableEntity', '', content)

    with open(file_path, 'w') as f:
        f.write(content)
