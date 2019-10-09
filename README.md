# YYProject.BytesSearch
A pure C# library provides bytes searching capabilities supports wildcards.

## Example

```
var source = new byte[] { 0, 1, 2, 3, 4, 5, 6, 0x17, };
int result;

//Found, result is 1.
result = BytesFinder.FindIndex(source, new byte[] { 1, 2, 3 });
result = new BytesFinder(new byte[] { 1, 2, 3 }).FindIndexIn(source);

//Not found, result is -1.
result = BytesFinder.FindIndex(source, new byte[] { 8, 9, 10 });
result = new BytesFinder(new byte[] { 8, 9, 10 }).FindIndexIn(source);

//Found, result is 1.
result = BytesFinder.FindIndex(source, "01 02 ?? 04");
result = new BytesFinder("01 02 ?? 04").FindIndexIn(source);

//Not found, result is -1.
result = BytesFinder.FindIndex(source, "01 02 ?? 03");
result = new BytesFinder("01 02 ?? 03").FindIndexIn(source);

//Found, result is 5.
result = BytesFinder.FindIndex(source, "05 06 ?7");
result = new BytesFinder("05 06 ?7").FindIndexIn(source);

//Found, result is 5.
result = BytesFinder.FindIndex(source, "05 06 1?");
result = new BytesFinder("05 06 1?").FindIndexIn(source);

//Not found, result is -1.
result = BytesFinder.FindIndex(source, "05 06 ?8");
result = BytesFinder.FindIndex(source, "05 06 2?");

//Found, result is 0.
result = BytesFinder.FindIndex(source, "?? ?? ?? ?? ?? ?? ?? ??");

//Not found, result is -1.
result = BytesFinder.FindIndex(source, "?? ?? ?? ?? ?? ?? ?? ?? ??");
```

## Pattern Rules
* Any space character (0x20) should be ignored.
* A byte number must be expressed as two-digit hexadecimal number, excludes any prefix or postfix.
* Question mark (0x3F) represents wildcard. Wildcards must be in pairs, or it has a leading or trailing hexadecimal number.

`"A102C3"`　　√

`"A1 02 C3"`　√

`"A102 C3"`　&nbsp;&nbsp;√

`"A1 ?? C3"`　√

`"A1 ?2 C3"`　√

`"A1 0? C3"`　√

`"A1 2 C3"`　　×

`"A1 02 195"`　×

 `"A1 ? C3"`　　×
 
 ## Speed Test

10byte ~ 16MB random data, a thousand times:

Debug: bytes pattern cost is 1309.7017 , string pattern cost is 948.357200000001 , wildcard string pattern cost is 32906.4641, Boyer Moore algorithm implementation is 1440.9692

Release: bytes pattern cost is 676.126699999998 , string pattern cost is 608.047799999999 , wildcard string pattern cost is 14577.6418, Boyer Moore algorithm implementation is 929.0357

P.S: BM implementation that I have used is here : https://stackoverflow.com/a/6964519

But notice that you should move these code to constructor:
```
ComputeLast();
ComputeMatch();
```
