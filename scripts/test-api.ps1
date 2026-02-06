$ErrorActionPreference = "Stop"

$base = "http://localhost:5068"
$userId = "0b1dde63-6b8b-45db-a206-d22816a6cf2e"
$proj = "src/Ayurveda-AI-Backend.WebAPI/Ayurveda-AI-Backend.WebAPI.csproj"

$jwtSecretLine = dotnet user-secrets list --project $proj | Where-Object { $_ -like "Supabase:JwtSecret*" } | Select-Object -First 1
if (-not $jwtSecretLine) {
    throw "Supabase:JwtSecret not found in user-secrets."
}

$jwtSecret = ($jwtSecretLine -split "=", 2)[1].Trim()

function ConvertTo-Base64Url {
    param([byte[]]$Bytes)
    [Convert]::ToBase64String($Bytes).TrimEnd("=").Replace("+", "-").Replace("/", "_")
}

function New-Jwt {
    param(
        [string]$Secret,
        [hashtable]$Payload
    )

    $header = @{ alg = "HS256"; typ = "JWT" } | ConvertTo-Json -Compress
    $payloadJson = $Payload | ConvertTo-Json -Compress
    $headerB64 = ConvertTo-Base64Url ([System.Text.Encoding]::UTF8.GetBytes($header))
    $payloadB64 = ConvertTo-Base64Url ([System.Text.Encoding]::UTF8.GetBytes($payloadJson))
    $unsigned = "$headerB64.$payloadB64"

    $hmac = [System.Security.Cryptography.HMACSHA256]::new([System.Text.Encoding]::UTF8.GetBytes($Secret))
    $signature = $hmac.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($unsigned))
    $sigB64 = ConvertTo-Base64Url $signature

    "$unsigned.$sigB64"
}

$now = [DateTimeOffset]::UtcNow
$payload = @{
    sub  = $userId
    email = "test.user@example.local"
    role = "admin"
    nbf = [int]$now.AddMinutes(-1).ToUnixTimeSeconds()
    exp = [int]$now.AddHours(1).ToUnixTimeSeconds()
    iat = [int]$now.ToUnixTimeSeconds()
}

$jwt = New-Jwt -Secret $jwtSecret -Payload $payload
$auth = @{ Authorization = "Bearer $jwt" }

function Invoke-Test {
    param(
        [string]$Method,
        [string]$Url,
        [object]$Body,
        [hashtable]$Headers
    )

    try {
        if ($null -ne $Body) {
            $json = $Body | ConvertTo-Json -Depth 6
            $resp = Invoke-RestMethod -Method $Method -Uri $Url -Headers $Headers -ContentType "application/json" -Body $json
            return @{ ok = $true; status = 200; body = $resp }
        }

        $resp = Invoke-RestMethod -Method $Method -Uri $Url -Headers $Headers
        return @{ ok = $true; status = 200; body = $resp }
    }
    catch {
        $resp = $_.Exception.Response
        if ($resp) {
            $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
            $text = $reader.ReadToEnd()
            return @{ ok = $false; status = [int]$resp.StatusCode; body = $text }
        }

        return @{ ok = $false; status = 0; body = $_.Exception.Message }
    }
}

$results = @()

$seed = Invoke-Test -Method "POST" -Url "$base/api/test/seed" -Body $null -Headers @{}
$results += @{ name = "seed"; result = $seed }
if (-not $seed.ok) {
    throw "Seed failed."
}

$couponId = $seed.body.couponId
$quizQuestionId = $seed.body.quizQuestionId

$results += @{ name = "get_policies"; result = Invoke-Test -Method "GET" -Url "$base/api/access/policies" -Body $null -Headers @{} }
$results += @{ name = "get_indicators"; result = Invoke-Test -Method "GET" -Url "$base/api/health/indicators" -Body $null -Headers @{} }
$results += @{ name = "get_poop_types"; result = Invoke-Test -Method "GET" -Url "$base/api/health/poop-types" -Body $null -Headers @{} }
$results += @{ name = "get_energy_levels"; result = Invoke-Test -Method "GET" -Url "$base/api/health/energy-levels" -Body $null -Headers @{} }
$results += @{ name = "get_quiz_questions"; result = Invoke-Test -Method "GET" -Url "$base/api/health/quiz-questions" -Body $null -Headers @{} }
$results += @{ name = "get_gemini_questions"; result = Invoke-Test -Method "GET" -Url "$base/api/health/gemini-questions" -Body $null -Headers @{} }
$results += @{ name = "get_articles"; result = Invoke-Test -Method "GET" -Url "$base/api/articles" -Body $null -Headers @{} }

$article = Invoke-Test -Method "POST" -Url "$base/api/articles" -Body @{
    title = "Test Article"
    summary = "Test summary"
    content = "Test content"
    tags = "test,api"
} -Headers $auth
$results += @{ name = "create_article"; result = $article }

$articleId = $article.body.id
if ($article.ok -and $articleId) {
    $results += @{ name = "get_article"; result = Invoke-Test -Method "GET" -Url "$base/api/articles/$articleId" -Body $null -Headers @{} }
    $results += @{ name = "update_article"; result = Invoke-Test -Method "PUT" -Url "$base/api/articles/$articleId" -Body @{
        title = "Updated Article"
        summary = "Updated summary"
        content = "Updated content"
        tags = "test,api,updated"
        status = 2
    } -Headers $auth }
    $results += @{ name = "delete_article"; result = Invoke-Test -Method "DELETE" -Url "$base/api/articles/$articleId" -Body $null -Headers $auth }
}

$results += @{ name = "upsert_profile"; result = Invoke-Test -Method "PUT" -Url "$base/api/users/$userId/profile" -Body @{
    userId = $userId
    firstName = "Test"
    lastName = "User"
    gender = 1
    dateOfBirth = "1990-01-01T00:00:00Z"
    weightLbs = 150
    heightFeet = 5
    heightInches = 8
    country = "US"
    timezone = "America/Denver"
    preferredLanguage = "en"
} -Headers $auth }
$results += @{ name = "get_profile"; result = Invoke-Test -Method "GET" -Url "$base/api/users/$userId/profile" -Body $null -Headers $auth }

$results += @{ name = "log_signal"; result = Invoke-Test -Method "POST" -Url "$base/api/health/signals" -Body @{
    userId = $userId
    signalType = 1
    signalValue = "Good"
    numericValue = 7.5
    reportedAt = $null
    source = "test"
} -Headers $auth }
$results += @{ name = "get_signals"; result = Invoke-Test -Method "GET" -Url "$base/api/health/signals/$userId" -Body $null -Headers $auth }
$results += @{ name = "save_vikriti"; result = Invoke-Test -Method "POST" -Url "$base/api/health/vikriti" -Body @{
    userId = $userId
    vataScore = 5
    pittaScore = 3
    kaphaScore = 2
    dominantDosha = 1
    reasonSummary = "Test run"
} -Headers $auth }
$results += @{ name = "save_prakriti"; result = Invoke-Test -Method "POST" -Url "$base/api/health/prakriti" -Body @{
    userId = $userId
    vataPercent = 40
    pittaPercent = 30
    kaphaPercent = 30
    prakritiLabel = 4
} -Headers $auth }
$results += @{ name = "log_prakriti_response"; result = Invoke-Test -Method "POST" -Url "$base/api/health/prakriti/response" -Body @{
    userId = $userId
    questionId = $quizQuestionId
    answerValue = "great"
} -Headers $auth }
$results += @{ name = "log_mcq_response"; result = Invoke-Test -Method "POST" -Url "$base/api/health/mcq" -Body @{
    userId = $userId
    questionId = $quizQuestionId
    answerValue = "great"
} -Headers $auth }

$results += @{ name = "analytics_dosha"; result = Invoke-Test -Method "GET" -Url "$base/api/health/analytics/dosha/$userId" -Body $null -Headers $auth }
$results += @{ name = "analytics_seasonal"; result = Invoke-Test -Method "GET" -Url "$base/api/health/analytics/seasonal/$userId" -Body $null -Headers $auth }
$results += @{ name = "analytics_trends"; result = Invoke-Test -Method "GET" -Url "$base/api/health/analytics/trends/$userId" -Body $null -Headers $auth }

$results += @{ name = "upsert_usage"; result = Invoke-Test -Method "POST" -Url "$base/api/access/usage" -Body @{
    userId = $userId
    date = (Get-Date).ToString("yyyy-MM-dd")
    chatsUsed = 1
    articlesUsed = 1
} -Headers $auth }
$results += @{ name = "redeem_coupon"; result = Invoke-Test -Method "POST" -Url "$base/api/access/coupons/redeem" -Body @{
    couponId = $couponId
    userId = $userId
} -Headers $auth }

$results += @{ name = "generate_articles"; result = Invoke-Test -Method "POST" -Url "$base/api/articles/generate" -Body @{
    userId = $userId
    timeOfDay = "morning"
    weather = "sunny"
    location = "Kathmandu"
} -Headers $auth }
$results += @{ name = "chat"; result = Invoke-Test -Method "POST" -Url "$base/api/chat" -Body @{
    userId = $userId
    message = "Hello from test suite"
} -Headers $auth }

foreach ($entry in $results) {
    $name = $entry.name
    $res = $entry.result
    if ($res.ok) {
        Write-Host ("OK`t" + $name)
    } else {
        Write-Host ("FAIL`t" + $name + "`t(status " + $res.status + ")")
        if ($res.body) {
            Write-Host ($res.body.ToString())
        }
    }
}
