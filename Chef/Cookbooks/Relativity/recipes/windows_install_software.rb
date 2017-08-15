log 'Starting software install'
start_time = DateTime.now
log "recipe_start_time(#{recipe_name}): #{start_time}"

# Install Software
include_recipe 'chocolatey'

# Install Notepad++
chocolatey_package 'notepadplusplus' do
  version '7.4.2'
  action :install
end

# Install Visual Studio 2015 Remote Debugger
chocolatey_package 'vs2015remotetools' do
  version '14.0.25424.0'
  action :install
end

end_time = DateTime.now
log "recipe_end_Time(#{recipe_name}): #{end_time}"
log "recipe_duration(#{recipe_name}): #{end_time.to_time - start_time.to_time} seconds"
log 'Finished software install'
