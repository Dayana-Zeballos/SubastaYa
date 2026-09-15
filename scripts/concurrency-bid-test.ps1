# Stress test de concurrencia (201 / 409)
#
# Dispara dos POST /api/auctions/{id}/bids identicos en paralelo sobre la misma subasta.
# Esperado: un 201 Created y un 409 Conflict (RowVersion).
#
# Uso (con la API corriendo en http://localhost:5240):
#   powershell -ExecutionPolicy Bypass -File .\scripts\concurrency-bid-test.ps1
#
# Opcional:
#   .\scripts\concurrency-bid-test.ps1 -BaseUrl http://localhost:5240 -AuctionId <guid>

param(
    [string] $BaseUrl = "http://localhost:5240",
    [string] $Email = "comprador2@test.com",
    [string] $Password = "Subasta2026!",
    [Guid] $AuctionId = [Guid]::Empty
)

$ErrorActionPreference = "Stop"

function Invoke-Json {
    param(
        [string] $Method,
        [string] $Url,
        [hashtable] $Headers = @{},
        [object] $Body = $null
    )

    $params = @{
        Method      = $Method
        Uri         = $Url
        Headers     = $Headers
        ContentType = "application/json"
    }

    if ($null -ne $Body) {
        $params.Body = ($Body | ConvertTo-Json -Compress)
    }

    try {
        return Invoke-RestMethod @params
    }
    catch {
        $response = $_.Exception.Response
        if ($null -eq $response) { throw }
        $reader = New-Object System.IO.StreamReader($response.GetResponseStream())
        $text = $reader.ReadToEnd()
        throw "HTTP $([int]$response.StatusCode): $text"
    }
}

Write-Host "== Login ($Email) =="
$login = Invoke-Json -Method POST -Url "$BaseUrl/api/auth/login" -Body @{
    email    = $Email
    password = $Password
}
$token = $login.token
if (-not $token) { throw "Login no devolvio token." }
$auth = @{ Authorization = "Bearer $token" }

if ($AuctionId -eq [Guid]::Empty) {
    Write-Host "== Buscando subasta activa =="
    $page = Invoke-Json -Method GET -Url "$BaseUrl/api/auctions?status=Active&pageSize=5"
    $auction = $page.items | Select-Object -First 1
    if (-not $auction) { throw "No hay subastas activas. Reinicia la API con seed fresco." }
    $AuctionId = [Guid]$auction.id
}

Write-Host "== Detalle de subasta $AuctionId =="
$detail = Invoke-Json -Method GET -Url "$BaseUrl/api/auctions/$AuctionId"
$amount = [math]::Round([decimal]$detail.nextMinimumBid, 2)

Write-Host "Subasta: $AuctionId"
Write-Host "Monto paralelo: $amount"

$bidUrl = "$BaseUrl/api/auctions/$AuctionId/bids"
$bodyJson = (@{ amount = $amount } | ConvertTo-Json -Compress)

$jobScript = {
    param($Url, $Token, $Json)
    try {
        $response = Invoke-WebRequest -Method POST -Uri $Url `
            -Headers @{ Authorization = "Bearer $Token" } `
            -ContentType "application/json" `
            -Body $Json `
            -UseBasicParsing
        return [int]$response.StatusCode
    }
    catch {
        $resp = $_.Exception.Response
        if ($null -ne $resp) { return [int]$resp.StatusCode }
        return -1
    }
}

Write-Host "== Disparando 2 pujas en paralelo =="
$job1 = Start-Job -ScriptBlock $jobScript -ArgumentList $bidUrl, $token, $bodyJson
$job2 = Start-Job -ScriptBlock $jobScript -ArgumentList $bidUrl, $token, $bodyJson
Wait-Job $job1, $job2 | Out-Null
$codes = @((Receive-Job $job1), (Receive-Job $job2))
Remove-Job $job1, $job2

Write-Host "Resultados: $($codes -join ', ')"

$has201 = $codes -contains 201
$has409 = $codes -contains 409

if ($has201 -and $has409) {
    Write-Host "OK: una puja entro (201) y la otra fue rechazada por concurrencia (409)."
    exit 0
}

Write-Host "FALLO: se esperaba un 201 y un 409. Codigos obtenidos: $($codes -join ', ')"
Write-Host "Tip: corre el script apenas levantas la API (seed fresco) y con una sola instancia."
exit 1
