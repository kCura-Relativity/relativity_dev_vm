log 'Starting Sql Server install'
start_time = DateTime.now
log "recipe_start_time(#{recipe_name}): #{start_time}"

# Install 7 zip software
include_recipe 'seven_zip::default'

# Extract Contents of iso file
seven_zip_archive "extract sql iso" do
  source    "#{node['sql']['install']['destination_folder']}\\#{node['sql']['install']['file_name']}"
  path      node['sql']['install']['destination_folder']
  overwrite true
  timeout   300
end

# Install xSQLServer powershell module
powershell_script 'install_xSQLServer_module' do
  code 'Install-Module -Name xSQLServer -RequiredVersion 7.1.0.0 -Force'
  not_if '(Get-Module -ListAvailable).Name -Contains \"xSQLServer\"'
end

# Install Carbon powershell module
powershell_script 'install_Carbon_module' do
  code 'Install-Module -Name Carbon -RequiredVersion 2.5.0 -Force'
  not_if '(Get-Module -ListAvailable).Name -Contains \"Carbon\"'
end

relativity_pscredential = ps_credential(node['windows']['user']['admin']['login'], node['windows']['user']['admin']['password'])

# Install Microsoft SQL Server
dsc_resource 'install_sql_server' do
  resource :xsqlserversetup
  property :features, 'SQLENGINE,FULLTEXT'
  property :instancename, node['sql']['instance_name']
  property :securitymode, 'SQL'
  property :sourcepath, 'C:/Chef_Install/Sql'
  property :updateenabled, 'False'
  property :agtsvcaccount, relativity_pscredential
  property :ftsvcaccount, relativity_pscredential
  property :sapwd, relativity_pscredential
  property :setupcredential, relativity_pscredential
  property :sqlsvcaccount, relativity_pscredential
  property :sqlsysadminaccounts, [node['windows']['user']['admin']['login']]
  timeout 3600
end

# Enable SQL TCP and Named Pipes remote connections
protocols = %w(Tcp Np)

protocols.each do |protocol|
  powershell_script "enable_protocol_#{protocol}_#{node['sql']['instance_name']}" do
    code <<-EOH
      # Import-Module sqlps
      [reflection.assembly]::LoadWithPartialName("Microsoft.SqlServer.Smo")
      [reflection.assembly]::LoadWithPartialName("Microsoft.SqlServer.SqlWmiManagement")
      $wmi = New-Object ('Microsoft.SqlServer.Management.Smo.Wmi.ManagedComputer')
      $uri = "ManagedComputer[@Name='#{node['windows']['hostname'].upcase}']/ ServerInstance[@Name='#{node['sql']['instance_name']}']/ServerProtocol[@Name='#{protocol}']"
      $protocol = $wmi.GetSmoObject($uri)
      $protocol.IsEnabled = $true
      $protocol.Alter()
      EOH
  end
end

powershell_script "restart_MSSQL$#{node['sql']['instance_name']}" do
  action :nothing
  code <<-EOH
    Restart-Service '#{node['sql']['instance_name']}' -Force
    EOH
end

node['sql']['directories'].each do |_key, path|
  directory path do
    recursive true
  end
end

service 'SQLBrowser' do
  action [:enable, :start]
end

end_time = DateTime.now
log "recipe_end_Time(#{recipe_name}): #{end_time}"
log "recipe_duration(#{recipe_name}): #{end_time.to_time - start_time.to_time} seconds"
log 'Finished Sql Server install'
