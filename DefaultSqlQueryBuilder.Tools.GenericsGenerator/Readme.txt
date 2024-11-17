This tool will take a base generic class (e.g. Foo<T>) and generate numerous new generic classes
with multiple type parameters (e.g. Foo<T1, T2>, Foo<T1, T2, T3>, etc.)

The input parameter is the path of the base generic class, based on which all the new classes
will be created.

By default, this project has a linked item "SqlQueryBuilder.01.cs", from the SqlQueryBuilder project,
so you could just Run the GenericsGenerator and it should automatically create other generic classes
in its bin/ folder, which you can copy/paste to the SqlQueryBuilder project overwriting the old files.

Running this tool will give you the following screen:
---
$ dotnet.exe GenericsGenerator.dll SqlQueryBuilder.01.cs
How much generics to create for the file 'SqlQueryBuilder.01.cs': 10
Done.
---

After this, inspect the /bin/Debug/ folder, for the generated .cs files.
