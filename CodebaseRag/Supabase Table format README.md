To create table in supabase run this query:
    
```sql
    CREATE TABLE code_functions (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        file_path TEXT NOT NULL,
        class_name TEXT NOT NULL,
        function_name TEXT NOT NULL,
        start_line INTEGER NOT NULL,
        end_line INTEGER NOT NULL,
        parameters JSONB,      -- Stores function parameters as JSON
        return_type TEXT,      -- Stores return type of the function
        comments TEXT,         -- Stores comments or docstrings associated with the function
        code_snippet TEXT NOT NULL, -- The original code snippet of the function
        embedding vector(1536) NOT NULL,    -- Embedding vector for similarity searches
        last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP, -- Last modified timestamp of the file
        fields JSONB           -- Stores fields for MonoBehaviour classes or other metadata
    );
```